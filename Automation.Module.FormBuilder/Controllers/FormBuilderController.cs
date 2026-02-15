using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Module.FormBuilder.Controllers;

/// <summary>
/// Form Builder Controller
/// </summary>
public class FormBuilderController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly IdentityDbContext _identityContext;
    private readonly Automation.Infrastructure.Services.IFormSchemaService _schemaService;
    private readonly Automation.Infrastructure.Services.IFormXmlService _xmlService;
    private readonly Automation.Infrastructure.Services.IWordDocumentService _wordService;
    private readonly IFormTableService _tableService;
    private readonly Automation.Core.Interfaces.IPermissionService _permissionService;
    private readonly Automation.Core.Interfaces.IFormButtonService _buttonService;
    private readonly Automation.Core.Interfaces.IDapperService _dapperService;
    private readonly ILogger<FormBuilderController>? _logger;

    public FormBuilderController(AutomationDbContext context,
        IdentityDbContext identityContext,
        Automation.Infrastructure.Services.IFormSchemaService schemaService,
        Automation.Infrastructure.Services.IFormXmlService xmlService,
        Automation.Infrastructure.Services.IWordDocumentService wordService,
        IFormTableService tableService,
        Automation.Core.Interfaces.IPermissionService permissionService,
        Automation.Core.Interfaces.IFormButtonService buttonService,
        Automation.Core.Interfaces.IDapperService dapperService,
        ILogger<FormBuilderController>? logger = null)
    {
        _context = context;
        _identityContext = identityContext;
        _schemaService = schemaService;
        _xmlService = xmlService;
        _wordService = wordService;
        _tableService = tableService;
        _permissionService = permissionService;
        _buttonService = buttonService;
        _dapperService = dapperService;
        _logger = logger;
    }

    // ... (Skipping unchanged methods) ...

    /// <summary>
    /// Save form fields
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/forms/{id}/fields")]
    public async Task<IActionResult> SaveFormFields(int id, [FromBody] List<FormFieldDto> fields)
    {
        if (fields == null)
            return BadRequest(new { success = false, error = "فهرست فیلدها ارسال نشده است" });

        var fieldsCount = fields.Count;
        var safeFields = fields.Where(f => f != null).ToList();

        var form = await _context.Forms
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (form == null)
            return NotFound();

        // Topological sort helps keep parent->child order stable in UI
        var sortedFields = TopologicalSortFields(safeFields);

        var existingFields = form.Fields.ToList();
        var existingById = existingFields.ToDictionary(f => f.Id);
        var incomingExistingIds = new HashSet<int>(sortedFields.Where(f => f.Id > 0).Select(f => f.Id));

        // Track new fields by temporary client IDs (negative ints)
        var tempIdToEntity = new Dictionary<int, FormField>();
        var pendingParentLinks = new List<(FormField field, int tempParentId)>();

        // Update existing and add new
        foreach (var fieldDto in sortedFields)
        {
            FormField field;
            FormField? existing = null;
            var isExisting = fieldDto.Id > 0 && existingById.TryGetValue(fieldDto.Id, out existing);
            if (isExisting && existing != null)
            {
                field = existing;
            }
            else
            {
                field = new FormField { FormId = id };
                _context.FormFields.Add(field);

                if (fieldDto.Id < 0)
                {
                    tempIdToEntity[fieldDto.Id] = field;
                }
            }

            // Parent handling: existing parent IDs are positive, temp parents are negative
            if (fieldDto.ParentFieldId.HasValue)
            {
                if (fieldDto.ParentFieldId.Value > 0)
                {
                    field.ParentFieldId = fieldDto.ParentFieldId;
                }
                else
                {
                    field.ParentFieldId = null;
                    pendingParentLinks.Add((field, fieldDto.ParentFieldId.Value));
                }
            }
            else
            {
                field.ParentFieldId = null;
            }

            field.FieldTypeId = fieldDto.FieldTypeId;
            field.FieldKey = fieldDto.FieldKey;
            field.Name = fieldDto.Name;
            field.LabelFa = fieldDto.LabelFa;
            field.LabelEn = fieldDto.LabelEn;
            field.Placeholder = fieldDto.Placeholder;
            field.HelpText = fieldDto.HelpText;
            field.DefaultValue = fieldDto.DefaultValue;
            field.DatabaseColumnName = fieldDto.DatabaseColumnName;
            field.DatabaseColumnType = fieldDto.DatabaseColumnType;
            field.IsRequired = fieldDto.IsRequired;
            field.IsReadOnly = fieldDto.IsReadOnly;
            field.IsDisabled = fieldDto.IsDisabled;
            field.IsHidden = fieldDto.IsHidden;
            field.DisplayOrder = fieldDto.DisplayOrder;
            field.Properties = fieldDto.Properties ?? "{}";
            field.Styles = fieldDto.Styles ?? "{}";
            field.CssClasses = fieldDto.CssClasses;
            field.InlineStyles = fieldDto.InlineStyles;
            field.Events = fieldDto.Events ?? "{}";
            field.RenderCondition = fieldDto.RenderCondition;
            field.Options = fieldDto.Options;
            field.DataSourceUrl = fieldDto.DataSourceUrl;
            field.MaxLength = fieldDto.MaxLength;
            field.IsNullable = fieldDto.IsNullable;
            field.HasIndex = fieldDto.HasIndex;
            field.CustomCss = fieldDto.CustomCss;
            field.CustomJs = fieldDto.CustomJs;
        }

        // Soft-delete fields that are removed on the client
        foreach (var existing in existingFields)
        {
            if (!incomingExistingIds.Contains(existing.Id))
            {
                existing.IsDeleted = true;
                existing.EditDate = DateTime.UtcNow;
            }
        }

        // Generate XML Metadata
        try 
        {
            var currentFields = form.Fields.Where(f => !f.IsDeleted).ToList();
            form.StructureXml = _xmlService.GenerateFormXml(form, currentFields);
        }
        catch(Exception ex)
        {
             // Log but continue
             Console.WriteLine($"Error generating XML: {ex.Message}");
        }

        form.UpdatedAt = DateTime.UtcNow;
        
        try
        {
            await _context.SaveChangesAsync();

            if (pendingParentLinks.Count > 0)
            {
                foreach (var (field, tempParentId) in pendingParentLinks)
                {
                    if (tempIdToEntity.TryGetValue(tempParentId, out var parent))
                    {
                        field.ParentFieldId = parent.Id;
                    }
                }
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving form fields for form {FormId}", id);
            return Json(new { success = false, error = "خطا در ذخیره فیلدها: " + ex.Message });
        }

        // 3. Update Database Schema
        try
        {
            var currentFields = form.Fields.Where(f => !f.IsDeleted).ToList();
            await _schemaService.EnsureTableStructureAsync(form, currentFields);
            form.IsTableCreated = true;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Schema update failed for form {FormId}", id);
            return Json(new { success = true, fieldCount = fieldsCount, warning = "Schema update failed: " + ex.Message });
        }

        return Json(new { success = true, fieldCount = fieldsCount });
    }

    // ... (Skipping unchanged methods) ...


    /// <summary>
    /// لیست فرم‌ها برای AJAX loading
    /// </summary>
    [HttpGet]
    [Route("formbuilder")]
    public IActionResult FormList()
    {
        return PartialView("FormList");
    }

    /// <summary>
    /// Form Builder Designer Page
    /// </summary>
    public async Task<IActionResult> Index(int? id = null)
    {
        const string fieldSql = @"
            SELECT 
                Id,
                Name,
                DisplayNameFa,
                DisplayNameEn,
                Category,
                Icon,
                Description,
                DefaultProperties,
                DefaultStyles,
                HtmlTemplate,
                RazorTemplate,
                IsContainer,
                CanCreateColumn,
                DisplayOrder,
                IsActive,
                IsDeleted
            FROM FieldTypes
            WHERE IsDeleted = 0 AND IsActive = 1
            ORDER BY Category, DisplayOrder";
        var fieldTypes = (await _dapperService.QueryAsync<FieldType>(fieldSql)).ToList();

        const string categorySql = @"
            SELECT Id, NameFa, NameEn, DisplayOrder, IsActive, IsDeleted
            FROM FormCategories
            WHERE IsDeleted = 0 AND IsActive = 1
            ORDER BY DisplayOrder";
        var categories = (await _dapperService.QueryAsync<FormCategory>(categorySql)).ToList();

        object? form = null;
        if (id.HasValue)
        {
            const string formSql = @"
                SELECT Id, FormCode, NameFa, NameEn, Description, CategoryId, Version, FormType,
                       BackgroundSettings, CanvasSize, CustomStyles, CustomScripts, DesignData, IsPublished, ButtonBarSettings
                FROM Forms
                WHERE IsDeleted = 0 AND Id = @Id";
            form = await _dapperService.QueryFirstOrDefaultAsync<dynamic>(formSql, new { Id = id.Value });
        }

        ViewBag.FieldTypes = fieldTypes;
        ViewBag.Categories = categories;
        ViewBag.Form = form;
        ViewBag.IsNew = !id.HasValue;

        return View();
    }

    /// <summary>
    /// List all forms
    /// </summary>
    public async Task<IActionResult> List()
    {
        const string sql = @"
            SELECT f.Id, f.FormCode, f.NameFa, f.NameEn, f.Version, f.IsPublished, f.CreatedAt, f.UpdatedAt,
                   c.Id AS CategoryId, c.NameFa AS CategoryNameFa
            FROM Forms f
            LEFT JOIN FormCategories c ON f.CategoryId = c.Id
            WHERE f.IsDeleted = 0
            ORDER BY ISNULL(f.UpdatedAt, f.CreatedAt) DESC";

        var rows = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        var forms = rows.Select(r => new Form
        {
            Id = r.Id,
            FormCode = r.FormCode,
            NameFa = r.NameFa,
            NameEn = r.NameEn,
            Version = r.Version,
            IsPublished = r.IsPublished,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            Category = r.CategoryId != null ? new FormCategory { Id = r.CategoryId, NameFa = r.CategoryNameFa } : null
        }).ToList();

        return View(forms);
    }

    #region API Endpoints

    /// <summary>
    /// Get all scalar functions
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/scalarfunctions")]
public async Task<IActionResult> GetScalarFunctions()
{
    try
    {
        const string sql = @"
            SELECT Id, Name, DisplayName, CAST(ReturnType AS NVARCHAR(50)) AS ReturnType
            FROM ScalarFunctions
            WHERE IsDeleted = 0 AND IsSystem = 0
            ORDER BY DisplayName";

        var functions = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        return Json(new { success = true, data = functions });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// Get all field types
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/fieldtypes")]
public async Task<IActionResult> GetFieldTypes()
{
    try
    {
        const string sql = @"
            SELECT 
                Id,
                Name,
                DisplayNameFa AS DisplayName,
                DisplayNameEn,
                Category,
                Icon,
                Description,
                DefaultProperties,
                DefaultStyles,
                HtmlTemplate,
                RazorTemplate,
                IsContainer,
                CanCreateColumn,
                DisplayOrder,
                IsActive
            FROM FieldTypes
            WHERE IsDeleted = 0
            ORDER BY DisplayNameFa";

        var types = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        return Json(new { success = true, data = types });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

        /// <summary>
    /// Get all forms
    /// </summary>
    [HttpGet]
    [Route("api/formbuilder/forms")]
    public async Task<IActionResult> GetForms([FromQuery] string? search = null, [FromQuery] bool? isPublished = null, [FromQuery] int? limit = null)
    {
        try
        {
            var topClause = (limit.HasValue && limit.Value > 0) ? "TOP (@Limit)" : string.Empty;
            var sql = $@"
                SELECT {topClause}
                    f.Id,
                    f.FormCode,
                    f.NameFa,
                    f.NameEn,
                    c.NameFa AS CategoryName,
                    f.Version,
                    f.IsPublished,
                    (SELECT COUNT(1) FROM FormFields ff WHERE ff.FormId = f.Id AND ff.IsDeleted = 0) AS FieldCount,
                    f.CreatedAt,
                    f.UpdatedAt
                FROM Forms f
                LEFT JOIN FormCategories c ON f.CategoryId = c.Id
                WHERE f.IsDeleted = 0
                  AND (@IsPublished IS NULL OR f.IsPublished = @IsPublished)
                  AND (@Search IS NULL OR f.NameFa LIKE '%' + @Search + '%' OR f.NameEn LIKE '%' + @Search + '%' OR f.FormCode LIKE '%' + @Search + '%')
                ORDER BY ISNULL(f.UpdatedAt, f.CreatedAt) DESC";

            var forms = (await _dapperService.QueryAsync<dynamic>(sql, new { Search = search, IsPublished = isPublished, Limit = limit })).ToList();
            return Json(forms);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting forms");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get forms for autocomplete
    /// </summary>
    [HttpGet]
    [Route("api/formbuilder/forms/autocomplete")]
    public async Task<IActionResult> GetFormsForAutocomplete([FromQuery] string? search = null, [FromQuery] int? limit = null)
    {
        try
        {
            var take = (limit.HasValue && limit.Value > 0) ? limit.Value : 20;
            const string sql = @"
                SELECT TOP (@Limit)
                    Id, FormCode, NameFa, NameEn
                FROM Forms
                WHERE IsDeleted = 0
                  AND (@Search IS NULL OR NameFa LIKE '%' + @Search + '%' OR NameEn LIKE '%' + @Search + '%' OR FormCode LIKE '%' + @Search + '%')
                ORDER BY NameFa";

            var forms = (await _dapperService.QueryAsync<dynamic>(sql, new { Search = search, Limit = take })).ToList();
            return Json(new { success = true, data = forms });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting forms for autocomplete");
            return Json(new { success = false, error = ex.Message });
        }
    }
/// <summary>
    /// Get forms organized in tree structure by category
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/forms/tree")]
public async Task<IActionResult> GetFormsTree()
{
    try
    {
        const string sql = @"
            SELECT f.Id, f.FormCode, f.NameFa, f.NameEn, c.NameFa AS CategoryName
            FROM Forms f
            LEFT JOIN FormCategories c ON f.CategoryId = c.Id
            WHERE f.IsDeleted = 0
            ORDER BY c.NameFa, f.NameFa";

        var forms = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        return Json(new { success = true, data = forms });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// Get form by ID
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/forms/{id}")]
public async Task<IActionResult> GetForm(int id)
{
    const string sql = @"
        SELECT 
            f.Id, f.FormCode, f.NameFa, f.NameEn, f.Description, f.CategoryId,
            c.NameFa AS CategoryName,
            f.Version, f.LayoutType, f.NumberingRule, f.NumberingPrefix, f.FormType,
            f.BackgroundSettings, f.CanvasSize, f.DatabaseTableName, f.IsTableCreated, f.StructureXml,
            f.CustomStyles, f.CustomScripts, f.BootstrapSettings, f.FormSettings, f.DesignData,
            f.IsPublished, f.PublishedAt, f.ButtonBarSettings
        FROM Forms f
        LEFT JOIN FormCategories c ON f.CategoryId = c.Id
        WHERE f.IsDeleted = 0 AND f.Id = @Id;

        SELECT 
            ff.Id, ff.FieldKey, ff.Name, ff.LabelFa, ff.LabelEn, ff.FieldTypeId,
            ft.Name AS FieldTypeName,
            ff.ParentFieldId, ff.DisplayOrder, ff.IsRequired, ff.IsNullable, ff.DefaultValue,
            ff.MaxLength, ff.DatabaseColumnName, ff.DatabaseColumnType, ff.Options, ff.Events
        FROM FormFields ff
        LEFT JOIN FieldTypes ft ON ff.FieldTypeId = ft.Id
        WHERE ff.FormId = @Id AND ff.IsDeleted = 0
        ORDER BY ff.DisplayOrder;

        SELECT * FROM FormScripts WHERE FormId = @Id AND IsDeleted = 0;
        SELECT * FROM FormStyles WHERE FormId = @Id AND IsDeleted = 0;
        SELECT * FROM FormValidations WHERE FormId = @Id AND IsDeleted = 0;";

    using var grid = await _dapperService.QueryMultipleAsync(sql, new { Id = id });
    var form = await grid.ReadFirstOrDefaultAsync<dynamic>();
    if (form == null)
        return NotFound();

    var fields = (await grid.ReadAsync<dynamic>()).ToList();
    var scripts = (await grid.ReadAsync<dynamic>()).ToList();
    var styles = (await grid.ReadAsync<dynamic>()).ToList();
    var validations = (await grid.ReadAsync<dynamic>()).ToList();

    return Json(new
    {
        form.Id,
        form.FormCode,
        form.NameFa,
        form.NameEn,
        form.Description,
        form.CategoryId,
        form.CategoryName,
        form.Version,
        form.LayoutType,
        form.NumberingRule,
        form.NumberingPrefix,
        form.FormType,
        form.BackgroundSettings,
        form.CanvasSize,
        form.DatabaseTableName,
        form.IsTableCreated,
        form.StructureXml,
        form.CustomStyles,
        form.CustomScripts,
        form.BootstrapSettings,
        form.FormSettings,
        form.DesignData,
        form.IsPublished,
        form.PublishedAt,
        form.ButtonBarSettings,
        Fields = fields,
        Scripts = scripts,
        Styles = styles,
        Validations = validations
    });
}

    /// <summary>
    /// Create new form
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/forms")]
    public async Task<IActionResult> CreateForm([FromBody] FormCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Use the exact form code provided by user, or generate if not provided
        string formCode = dto.FormCode;
        if (string.IsNullOrEmpty(formCode))
        {
            // Generate if not provided
            formCode = GenerateFormCode();
        }
        else
        {
            // Check if provided code already exists (excluding deleted forms for same form)
            var codeExists = await _context.Forms.AnyAsync(f => f.FormCode == formCode && !f.IsDeleted);
            if (codeExists)
            {
                // Only regenerate if it's a different form (not updating current form)
                // For new forms, generate a new code
                formCode = GenerateFormCode();
                _logger?.LogWarning("Provided form code {Code} already exists, generated new code: {NewCode}", dto.FormCode, formCode);
            }
        }
        
        // Final uniqueness check right before saving (handles race conditions)
        var finalCheck = await _context.Forms.AnyAsync(f => f.FormCode == formCode && !f.IsDeleted);
        if (finalCheck)
        {
            // Regenerate if somehow still duplicate
            formCode = GenerateFormCode();
            _logger?.LogWarning("Race condition detected, regenerated form code to: {NewCode}", formCode);
        }

        var form = new Form
        {
            FormCode = formCode,
            NameFa = dto.NameFa,
            NameEn = dto.NameEn ?? dto.NameFa,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            LayoutType = dto.LayoutType,
            NumberingRule = dto.NumberingRule ?? "",
            NumberingPrefix = dto.NumberingPrefix ?? "",
            CanvasSize = dto.CanvasSize ?? "{\"width\":\"100%\",\"height\":\"auto\"}",
            BackgroundSettings = dto.BackgroundSettings ?? "{}",
            CustomStyles = dto.CustomStyles ?? "",
            CustomScripts = dto.CustomScripts ?? "",
            DatabaseTableName = dto.DatabaseTableName ?? $"Frm_{dto.NameEn?.Replace(" ", "") ?? Guid.NewGuid().ToString("N")[..8]}",
            DesignData = dto.DesignData != null ? System.Text.Json.JsonSerializer.Serialize(dto.DesignData) : null
        };

        _context.Forms.Add(form);
        await _context.SaveChangesAsync();

        // Save FormDesign
        if (dto.DesignData != null)
        {
            var formDesign = new FormDesign
            {
                FormId = form.Id,
                DesignData = System.Text.Json.JsonSerializer.Serialize(dto.DesignData),
                CustomScripts = dto.CustomScripts,
                CustomStyles = dto.CustomStyles,
                FormSettings = dto.FormSettings != null ? System.Text.Json.JsonSerializer.Serialize(dto.FormSettings) : null,
                Version = 1,
                IsActiveVersion = true
            };
            _context.FormDesigns.Add(formDesign);
            await _context.SaveChangesAsync();
        }

        // Create database table if requested
        if (dto.CreateTable == true)
        {
            await _tableService.CreateFormTableAsync(form);
        }

        // ایجاد خودکار مجوزهای فرم
        try
        {
            await _permissionService.CreateFormPermissionsAsync(form.Id, form.NameFa);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the form creation
            if (_logger != null)
                _logger.LogError(ex, "Error creating form permissions for form {FormId}", form.Id);
        }

        return Json(new { success = true, id = form.Id, formCode = form.FormCode });
    }

    /// <summary>
    /// Update form
    /// </summary>
    [HttpPut]
    [Route("api/formbuilder/forms/{id}")]
    public async Task<IActionResult> UpdateForm(int id, [FromBody] FormUpdateDto dto)
    {
        var form = await _context.Forms.FindAsync(id);
        if (form == null)
            return NotFound();

        if (dto.NameFa != null) form.NameFa = dto.NameFa;
        if (dto.NameEn != null) form.NameEn = dto.NameEn;
        if (dto.Description != null) form.Description = dto.Description;
        if (dto.CategoryId.HasValue) form.CategoryId = dto.CategoryId;
        if (dto.LayoutType.HasValue) form.LayoutType = dto.LayoutType.Value;
        if (dto.NumberingRule != null) form.NumberingRule = dto.NumberingRule;
        if (dto.NumberingPrefix != null) form.NumberingPrefix = dto.NumberingPrefix;
        if (dto.CanvasSize != null) form.CanvasSize = dto.CanvasSize;
        if (dto.BackgroundSettings != null) form.BackgroundSettings = dto.BackgroundSettings;
        if (dto.CustomStyles != null) form.CustomStyles = dto.CustomStyles;
        if (dto.CustomScripts != null) form.CustomScripts = dto.CustomScripts;
        if (dto.DesignData != null)
        {
            form.DesignData = System.Text.Json.JsonSerializer.Serialize(dto.DesignData);
            
            // Update or create FormDesign
            var formDesign = await _context.FormDesigns
                .Where(fd => fd.FormId == id && fd.IsActiveVersion)
                .FirstOrDefaultAsync();
                
            if (formDesign == null)
            {
                // Get max version
                var maxVersion = await _context.FormDesigns
                    .Where(fd => fd.FormId == id)
                    .Select(fd => fd.Version)
                    .DefaultIfEmpty(0)
                    .MaxAsync();
                    
                // Deactivate old versions
                await _context.FormDesigns
                    .Where(fd => fd.FormId == id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(fd => fd.IsActiveVersion, false));
                
                formDesign = new FormDesign
                {
                    FormId = id,
                    Version = maxVersion + 1,
                    IsActiveVersion = true
                };
                _context.FormDesigns.Add(formDesign);
            }
            
            formDesign.DesignData = System.Text.Json.JsonSerializer.Serialize(dto.DesignData);
            formDesign.CustomScripts = dto.CustomScripts ?? formDesign.CustomScripts;
            formDesign.CustomStyles = dto.CustomStyles ?? formDesign.CustomStyles;
            formDesign.FormSettings = dto.FormSettings != null ? System.Text.Json.JsonSerializer.Serialize(dto.FormSettings) : formDesign.FormSettings;
            formDesign.EditDate = DateTime.UtcNow;
        }
        
        form.UpdatedAt = DateTime.UtcNow;
        form.EditDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Update table if requested
        if (dto.UpdateTable == true)
        {
            await _tableService.UpdateFormTableAsync(form);
        }

        return Json(new { success = true });
    }



    /// <summary>
    /// Delete form
    /// </summary>
    [HttpDelete]
    [Route("api/formbuilder/forms/{id}")]
    public async Task<IActionResult> DeleteForm(int id)
    {
        var form = await _context.Forms.FindAsync(id);
        if (form == null)
            return NotFound();

        form.IsDeleted = true;
        form.UpdatedAt = DateTime.UtcNow;

        // Cascade delete: soft-delete form permissions and permission group (Identity context)
        var groupCode = $"FRMB_{form.FormCode}";
        var permGroup = await _identityContext.PermissionGroups
            .FirstOrDefaultAsync(pg => pg.GroupCode == groupCode && !pg.IsDeleted);

        if (permGroup != null)
        {
            var formPerms = await _identityContext.Permissions
                .Where(p => p.PermissionGroupId == permGroup.Id && !p.IsDeleted)
                .ToListAsync();

            foreach (var perm in formPerms)
            {
                var rolePerms = await _identityContext.RolePermissions
                    .Where(rp => rp.PermissionId == perm.Id && !rp.IsDeleted)
                    .ToListAsync();
                foreach (var rp in rolePerms)
                {
                    rp.IsDeleted = true;
                    rp.EditDate = DateTime.UtcNow;
                }

                var groupPerms = await _identityContext.GroupPermissions
                    .Where(gp => gp.PermissionId == perm.Id && !gp.IsDeleted)
                    .ToListAsync();
                foreach (var gp in groupPerms)
                {
                    gp.IsDeleted = true;
                    gp.EditDate = DateTime.UtcNow;
                }

                perm.IsDeleted = true;
                perm.EditDate = DateTime.UtcNow;
            }

            permGroup.IsDeleted = true;
            permGroup.EditDate = DateTime.UtcNow;
        }

        await _identityContext.SaveChangesAsync();
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// Preview form
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/forms/{id}/preview")]
public async Task<IActionResult> PreviewForm(int id)
{
    const string sql = @"
        SELECT Id, NameFa, Description, DesignData, CanvasSize
        FROM Forms
        WHERE IsDeleted = 0 AND Id = @Id";

    var form = await _dapperService.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
    if (form == null)
        return NotFound();

    ViewBag.Form = form;
    return View("Preview");
}

    /// <summary>
    /// Get all print templates for a form
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/forms/{formId}/printtemplates")]
public async Task<IActionResult> GetPrintTemplates(int formId)
{
    const string sql = @"
        SELECT Id, Name, Description, FilePath, IsDefault, DisplayOrder, CreatedAt
        FROM PrintTemplates
        WHERE FormId = @FormId AND IsDeleted = 0
        ORDER BY DisplayOrder, Name";

    var templates = (await _dapperService.QueryAsync<dynamic>(sql, new { FormId = formId })).ToList();
    return Json(templates);
}

    /// <summary>
    /// Get print template by ID
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/printtemplates/{id}")]
public async Task<IActionResult> GetPrintTemplate(int id)
{
    const string sql = @"
        SELECT Id, FormId, Name, Description, FilePath, IsDefault, DisplayOrder
        FROM PrintTemplates
        WHERE Id = @Id AND IsDeleted = 0";

    var template = await _dapperService.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });
    if (template == null)
        return NotFound();

    return Json(template);
}

    /// <summary>
    /// Create or update print template
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/forms/{formId}/printtemplates")]
    public async Task<IActionResult> SavePrintTemplate(int formId, [FromBody] PrintTemplateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var form = await _context.Forms.FindAsync(formId);
        if (form == null)
            return NotFound();

        PrintTemplate template;
        if (dto.Id.HasValue && dto.Id.Value != 0)
        {
            template = await _context.PrintTemplates.FindAsync(dto.Id.Value);
            if (template == null)
                return NotFound();
        }
        else
        {
            template = new PrintTemplate
            {
                FormId = formId
            };
            _context.PrintTemplates.Add(template);
        }

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.FilePath = dto.FilePath;
        template.IsDefault = dto.IsDefault;
        template.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();

        return Json(new { success = true, id = template.Id });
    }

    /// <summary>
    /// Delete print template
    /// </summary>
    [HttpDelete]
    [Route("api/formbuilder/printtemplates/{id}")]
    public async Task<IActionResult> DeletePrintTemplate(int id)
    {
        var template = await _context.PrintTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// Upload print template Word file
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/printtemplates/{id}/upload")]
    public async Task<IActionResult> UploadPrintTemplateFile(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("فایل انتخاب نشده است");

        if (!file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("فقط فایل‌های Word (.docx) مجاز هستند");

        var template = await _context.PrintTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "printtemplates");
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var fileName = $"{template.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}_{file.FileName}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        template.FilePath = $"/uploads/printtemplates/{fileName}";
        await _context.SaveChangesAsync();

        return Json(new { success = true, filePath = template.FilePath });
    }

    /// <summary>
    /// Generate Word document from template with form data
    /// </summary>
    [HttpPost]
[Route("api/formbuilder/printtemplates/{id}/generate")]
public async Task<IActionResult> GenerateWordDocument(int id, [FromBody] Dictionary<string, object> formData)
{
    const string templateSql = @"
        SELECT Id, FormId, FilePath
        FROM PrintTemplates
        WHERE Id = @Id AND IsDeleted = 0";

    var template = await _dapperService.QueryFirstOrDefaultAsync<dynamic>(templateSql, new { Id = id });
    if (template == null)
        return NotFound();

    if (string.IsNullOrEmpty((string?)template.FilePath))
        return BadRequest("???? ??? ???? Word ?????");

    const string fieldsSql = @"
        SELECT Id, FormId, Name, DatabaseColumnName
        FROM FormFields
        WHERE FormId = @FormId AND IsDeleted = 0";

    var fieldRows = (await _dapperService.QueryAsync<dynamic>(fieldsSql, new { FormId = (int)template.FormId })).ToList();
    var fields = fieldRows.Select(f => new FormField
    {
        Id = f.Id,
        FormId = f.FormId,
        Name = f.Name,
        DatabaseColumnName = f.DatabaseColumnName
    }).ToList();

    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ((string)template.FilePath).TrimStart('/'));
    if (!System.IO.File.Exists(templatePath))
        return NotFound("???? ???? ???? ???");

    // Generate output file path
    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp", $"output_{Guid.NewGuid()}.docx");
    var outputDir = Path.GetDirectoryName(outputPath);
    if (!Directory.Exists(outputDir))
        Directory.CreateDirectory(outputDir);

    try
    {
        var resultPath = await _wordService.ReplaceBookmarksAsync(templatePath, formData, fields);
        var relativePath = $"/temp/{Path.GetFileName(resultPath)}";
        return Json(new { success = true, filePath = relativePath });
    }
    catch (NotImplementedException)
    {
        System.IO.File.Copy(templatePath, outputPath, true);
        var relativePath = $"/temp/{Path.GetFileName(outputPath)}";
        return Json(new { success = true, filePath = relativePath, warning = "Word processing not implemented" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// Get all database tables and views (forms created by form builder)
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/database/tables")]
public async Task<IActionResult> GetDatabaseTables()
{
    try
    {
        const string sql = @"
            SELECT 
                f.Id,
                f.NameFa,
                f.NameEn,
                f.DatabaseTableName,
                'Form' AS Type,
                (SELECT COUNT(1) FROM FormFields ff WHERE ff.FormId = f.Id AND ff.IsDeleted = 0) AS FieldCount
            FROM Forms f
            WHERE f.IsDeleted = 0
              AND f.IsTableCreated = 1
              AND f.DatabaseTableName IS NOT NULL
              AND f.DatabaseTableName <> ''
            ORDER BY f.NameFa";

        var forms = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();

        // TODO: Also get database views from system
        // For now, we'll only return forms

        return Json(new { success = true, tables = forms });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// Get table columns information
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/database/tables/{tableName}/columns")]
public async Task<IActionResult> GetTableColumns(string tableName)
{
    try
    {
        if (string.IsNullOrWhiteSpace(tableName) || !System.Text.RegularExpressions.Regex.IsMatch(tableName, "^[A-Za-z0-9_]+$"))
            return BadRequest(new { success = false, error = "??? ???? ??????? ???" });

        const string formSql = @"
            SELECT TOP 1 Id
            FROM Forms
            WHERE IsDeleted = 0 AND DatabaseTableName = @TableName";

        var formId = await _dapperService.QueryFirstOrDefaultAsync<int?>(formSql, new { TableName = tableName });
        if (!formId.HasValue)
            return NotFound(new { success = false, error = "???? ???? ???" });

        const string columnsSql = @"
            SELECT 
                ff.Id,
                ISNULL(ff.DatabaseColumnName, ff.LabelEn) AS ColumnName,
                ff.LabelFa,
                ISNULL(ff.LabelEn, ff.DatabaseColumnName) AS LabelEn,
                ISNULL(ff.DatabaseColumnType, 'nvarchar') AS DataType,
                ff.IsRequired,
                ff.IsNullable,
                ff.MaxLength
            FROM FormFields ff
            WHERE ff.FormId = @FormId AND ff.IsDeleted = 0
            ORDER BY ff.DisplayOrder";

        var columns = (await _dapperService.QueryAsync<dynamic>(columnsSql, new { FormId = formId.Value })).ToList();
        return Json(new { success = true, columns });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// Get all stored procedures and functions
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/database/functions")]
public async Task<IActionResult> GetDatabaseFunctions()
{
    try
    {
        const string sql = @"
            SELECT Id, Name, DisplayName, CAST(ReturnType AS NVARCHAR(50)) AS ReturnType
            FROM ScalarFunctions
            WHERE IsDeleted = 0 AND IsSystem = 0
            ORDER BY DisplayName";

        var functions = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        return Json(new { success = true, functions });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    #endregion

    #region === Action Buttons API ===

    /// <summary>
    /// لیست انواع دکمه‌های پیشفرض
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/buttontypes")]
public async Task<IActionResult> GetButtonTypes()
{
    try
    {
        const string sql = @"
            SELECT Id, ButtonCode, NameFa, NameEn, DefaultIcon, DefaultColor, Category, ActionHandler, OpensModal, ModalId, DisplayOrder, Description
            FROM FormButtonTypes
            WHERE IsDeleted = 0
            ORDER BY DisplayOrder";

        var types = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        return Json(new { success = true, data = types });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// لیست قالب‌های استایل دکمه
    /// </summary>
    [HttpGet]
[Route("api/formbuilder/buttonstylepresets")]
public async Task<IActionResult> GetButtonStylePresets()
{
    try
    {
        const string sql = @"
            SELECT Id, PresetName, Description, StyleDefinition, IsSystem, DisplayOrder
            FROM ButtonStylePresets
            WHERE IsDeleted = 0
            ORDER BY DisplayOrder";

        var presets = (await _dapperService.QueryAsync<dynamic>(sql)).ToList();
        return Json(new { success = true, data = presets });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, error = ex.Message });
    }
}

    /// <summary>
    /// دریافت تنظیمات دکمه‌های یک فرم
    /// </summary>
    [HttpGet]
    [Route("api/formbuilder/forms/{formId}/buttons")]
    public async Task<IActionResult> GetFormButtons(int formId)
    {
        try
        {
            var config = await _buttonService.GetFormButtonConfigAsync(formId);
            return Json(new { success = true, data = config });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ذخیره تنظیمات دکمه‌های یک فرم
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/forms/{formId}/buttons")]
    public async Task<IActionResult> SaveFormButtons(int formId, [FromBody] Automation.Core.DTOs.FormButtonConfigDto config)
    {
        try
        {
            var result = await _buttonService.SaveFormButtonConfigAsync(formId, config);
            if (!result)
                return NotFound(new { success = false, error = "فرم یافت نشد" });

            return Json(new { success = true, message = "دکمه‌ها با موفقیت ذخیره شدند" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// انتشار فرم (تبدیل از پیش‌نویس به فرم قابل استفاده)
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/forms/{formId}/publish")]
    public async Task<IActionResult> PublishForm(int formId)
    {
        try
        {
            var form = await _context.Forms.FindAsync(formId);
            if (form == null)
                return NotFound(new { success = false, error = "فرم یافت نشد" });

            if (form.IsPublished)
                return BadRequest(new { success = false, error = "این فرم قبلاً منتشر شده است" });

            form.IsPublished = true;
            form.PublishedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

        return Json(new { success = true, message = "فرم با موفقیت منتشر شد و اکنون قابل استفاده است" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// لغو انتشار فرم (برگشت به حالت پیش‌نویس)
    /// </summary>
    [HttpPost]
    [Route("api/formbuilder/forms/{formId}/unpublish")]
    public async Task<IActionResult> UnpublishForm(int formId)
    {
        try
        {
            var form = await _context.Forms.FindAsync(formId);
            if (form == null)
                return NotFound(new { success = false, error = "فرم یافت نشد" });

            form.IsPublished = false;
            form.PublishedAt = null;
            await _context.SaveChangesAsync();

        return Json(new { success = true, message = "انتشار فرم لغو شد" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    #endregion

    private string GenerateFormCode()
    {
        // Get the highest form code number (including deleted forms to avoid conflicts)
        var lastForm = _context.Forms
            .Where(f => f.FormCode.StartsWith("FRM-"))
            .OrderByDescending(f => f.FormCode)
            .FirstOrDefault();

        int nextNumber = 1;
        if (lastForm != null)
        {
            var codePart = lastForm.FormCode.Substring(4); // Remove "FRM-"
            if (int.TryParse(codePart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        // Ensure the generated code is unique (check synchronously for now)
        // Note: In high-concurrency scenarios, consider using database-level unique constraint handling
        string newCode;
        int attempts = 0;
        do
        {
            newCode = $"FRM-{nextNumber:D4}";
            var exists = _context.Forms.Any(f => f.FormCode == newCode && !f.IsDeleted);
            if (!exists)
                break;
            
            nextNumber++;
            attempts++;
            if (attempts > 100) // Safety limit
            {
                // Fallback: use timestamp-based code
                newCode = $"FRM-{DateTime.UtcNow:yyyyMMddHHmmss}";
                break;
            }
        } while (true);

        return newCode;
    }

    /// <summary>
    /// Topological sort of form fields to ensure parents are inserted before children
    /// Handles multi-level hierarchies correctly
    /// </summary>
    private List<FormFieldDto> TopologicalSortFields(List<FormFieldDto> fields)
    {
        if (fields == null || !fields.Any())
            return fields ?? new List<FormFieldDto>();

        // Create a map of field IDs to field DTOs
        var fieldMap = new Dictionary<int, FormFieldDto>();
        foreach (var field in fields)
        {
            var fieldId = field.Id;
            // Ensure each field has a valid ID
            if (field.Id == 0)
            {
                field.Id = fieldId;
            }
            fieldMap[fieldId] = field;
        }

        // Build dependency graph: child -> parent
        var visited = new HashSet<int>();
        var result = new List<FormFieldDto>();
        var processing = new HashSet<int>(); // Detect cycles

        void Visit(int fieldId)
        {
            if (visited.Contains(fieldId))
                return;

            if (processing.Contains(fieldId))
            {
                // Cycle detected - log warning and skip
                _logger?.LogWarning("Cycle detected in field hierarchy for field {FieldId}, breaking cycle", fieldId);
                visited.Add(fieldId);
                if (fieldMap.ContainsKey(fieldId))
                {
                    var cycleField = fieldMap[fieldId];
                    cycleField.ParentFieldId = null; // Break the cycle
                    result.Add(cycleField);
                }
                return;
            }

            if (!fieldMap.ContainsKey(fieldId))
                return;

            processing.Add(fieldId);
            var currentField = fieldMap[fieldId];

            // Visit parent first if it exists
            if (currentField.ParentFieldId.HasValue && currentField.ParentFieldId.Value != 0)
            {
                var parentId = currentField.ParentFieldId.Value;
                if (fieldMap.ContainsKey(parentId))
                {
                    Visit(parentId);
                }
                else
                {
                    // Parent not in current batch - set to null
                    _logger?.LogWarning("Parent field {ParentId} not found in batch for field {FieldId}, setting ParentFieldId to null", 
                        parentId, fieldId);
                    currentField.ParentFieldId = null;
                }
            }

            processing.Remove(fieldId);
            visited.Add(fieldId);
            result.Add(currentField);
        }

        // Visit all fields
        foreach (var fieldId in fieldMap.Keys)
        {
            Visit(fieldId);
        }

        return result;
    }
}









































