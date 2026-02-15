using Automation.Core.Entities;
using Automation.Core.Interfaces;
using Automation.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Automation.Web.Controllers;

/// <summary>
/// Controller for using forms (CRUD operations using Stored Procedures)
/// </summary>
public class FormDataController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FormDataController> _logger;
    private readonly IFormButtonService _buttonService;
    private readonly ICurrentContextSetter _contextSetter;

    public FormDataController(
        AutomationDbContext context,
        IConfiguration configuration,
        ILogger<FormDataController> logger,
        IFormButtonService buttonService,
        ICurrentContextSetter contextSetter)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _buttonService = buttonService;
        _contextSetter = contextSetter;
    }

    /// <summary>
    /// Get form by ID for viewing/editing
    /// </summary>
    [HttpGet]
    [Route("forms/{formId}/view")]
    public async Task<IActionResult> ViewForm(int formId)
    {
        var form = await _context.Forms
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == formId && !f.IsDeleted);

        if (form == null)
            return NotFound();

        _contextSetter.SetFormContext(formId, null);

        // Get active form design
        var formDesign = await _context.FormDesigns
            .Where(fd => fd.FormId == formId && fd.IsActiveVersion && !fd.IsDeleted)
            .FirstOrDefaultAsync();

        // Get button configuration
        var buttonConfig = await _buttonService.GetFormButtonConfigAsync(formId);

        ViewBag.Form = form;
        ViewBag.FormDesign = formDesign;
        ViewBag.DesignData = formDesign?.DesignData ?? form.DesignData;
        ViewBag.ButtonConfig = buttonConfig;
        ViewBag.FormId = formId;

        return View("FormView");
    }

    /// <summary>
    /// Get all form data records (using SP)
    /// </summary>
    [HttpGet]
    [Route("api/forms/{formId}/data")]
    public async Task<IActionResult> GetFormData(int formId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var form = await _context.Forms.FindAsync(formId);
        if (form == null || string.IsNullOrEmpty(form.DatabaseTableName))
            return NotFound();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return StatusCode(500, new { error = "Connection string not found" });

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SP_{form.DatabaseTableName}_GetAll", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@PageNumber", page);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@SearchTerm", (object?)search ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();

            var records = new List<Dictionary<string, object>>();
            while (await reader.ReadAsync())
            {
                var record = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    record[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                records.Add(record);
            }

            await reader.NextResultAsync(); // Get total count
            int totalCount = 0;
            if (await reader.ReadAsync())
            {
                totalCount = reader.GetInt32(0);
            }

            return Json(new
            {
                success = true,
                data = records,
                totalCount,
                page,
                pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form data for form {FormId}", formId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get form data record by ID (using SP)
    /// </summary>
    [HttpGet]
    [Route("api/forms/{formId}/data/{id}")]
    public async Task<IActionResult> GetFormDataById(int formId, int id)
    {
        _contextSetter.SetFormContext(formId, id);
        var form = await _context.Forms.FindAsync(formId);
        if (form == null || string.IsNullOrEmpty(form.DatabaseTableName))
            return NotFound();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return StatusCode(500, new { error = "Connection string not found" });

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SP_{form.DatabaseTableName}_GetById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var record = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    record[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                return Json(new { success = true, data = record });
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form data by ID {Id} for form {FormId}", id, formId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Insert form data record (using SP)
    /// </summary>
    [HttpPost]
    [Route("api/forms/{formId}/data")]
    public async Task<IActionResult> InsertFormData(int formId, [FromBody] Dictionary<string, object> data)
    {
        _contextSetter.SetFormContext(formId, null);
        var form = await _context.Forms.FindAsync(formId);
        if (form == null || string.IsNullOrEmpty(form.DatabaseTableName))
            return NotFound();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return StatusCode(500, new { error = "Connection string not found" });

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SP_{form.DatabaseTableName}_Insert", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Get form fields to build parameters
            var fields = await _context.FormFields
                .Where(f => f.FormId == formId && !f.IsDeleted && !string.IsNullOrEmpty(f.DatabaseColumnName))
                .ToListAsync();

            foreach (var field in fields)
            {
                if (data.ContainsKey(field.DatabaseColumnName!))
                {
                    var value = data[field.DatabaseColumnName!];
                    command.Parameters.AddWithValue($"@{field.DatabaseColumnName}", value ?? DBNull.Value);
                }
            }

            // Add creator info (should come from authentication context)
            command.Parameters.AddWithValue("@CreatorUserId", (object?)data.GetValueOrDefault("CreatorUserId") ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatorRoleId", (object?)data.GetValueOrDefault("CreatorRoleId") ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            var newId = result?.ToString();

            return Json(new { success = true, id = newId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting form data for form {FormId}", formId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update form data record (using SP)
    /// </summary>
    [HttpPut]
    [Route("api/forms/{formId}/data/{id}")]
    public async Task<IActionResult> UpdateFormData(int formId, int id, [FromBody] Dictionary<string, object> data)
    {
        _contextSetter.SetFormContext(formId, id);
        var form = await _context.Forms.FindAsync(formId);
        if (form == null || string.IsNullOrEmpty(form.DatabaseTableName))
            return NotFound();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return StatusCode(500, new { error = "Connection string not found" });

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SP_{form.DatabaseTableName}_Update", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", id);

            // Get form fields to build parameters
            var fields = await _context.FormFields
                .Where(f => f.FormId == formId && !f.IsDeleted && !string.IsNullOrEmpty(f.DatabaseColumnName))
                .ToListAsync();

            foreach (var field in fields)
            {
                if (data.ContainsKey(field.DatabaseColumnName!))
                {
                    var value = data[field.DatabaseColumnName!];
                    command.Parameters.AddWithValue($"@{field.DatabaseColumnName}", value ?? DBNull.Value);
                }
            }

            var rowsAffected = await command.ExecuteScalarAsync();

            return Json(new { success = true, rowsAffected });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating form data {Id} for form {FormId}", id, formId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete form data record (using SP - soft delete)
    /// </summary>
    [HttpDelete]
    [Route("api/forms/{formId}/data/{id}")]
    public async Task<IActionResult> DeleteFormData(int formId, int id)
    {
        var form = await _context.Forms.FindAsync(formId);
        if (form == null || string.IsNullOrEmpty(form.DatabaseTableName))
            return NotFound();

        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            return StatusCode(500, new { error = "Connection string not found" });

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SP_{form.DatabaseTableName}_Delete", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await command.ExecuteScalarAsync();

            return Json(new { success = true, rowsAffected });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting form data {Id} for form {FormId}", id, formId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}




