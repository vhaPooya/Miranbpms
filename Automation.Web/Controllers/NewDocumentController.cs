using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// Controller for NewDocument (ایجاد مدرک جدید)
/// </summary>
public class NewDocumentController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<NewDocumentController> _logger;

    public NewDocumentController(
        AutomationDbContext context,
        IPermissionService permissionService,
        ISessionService sessionService,
        ILogger<NewDocumentController> logger)
    {
        _context = context;
        _permissionService = permissionService;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه اصلی ایجاد مدرک جدید
    /// </summary>
    [HttpGet]
    [Route("newdocument")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    /// <summary>
    /// دریافت لیست فرم‌های قابل دسترس
    /// </summary>
    [HttpGet]
    [Route("api/newdocument/forms")]
    public async Task<IActionResult> GetAvailableForms([FromQuery] string? search = null, [FromQuery] int? categoryId = null)
    {
        try
        {
            var userId = _sessionService.GetCurrentUserId();

            var query = _context.Forms
                .Include(f => f.Category)
                .Where(f => f.IsPublished && !f.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.NameFa.Contains(search) || f.NameEn.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId);
            }

            var forms = await query
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new
                {
                    f.Id,
                    f.FormCode,
                    f.NameFa,
                    f.NameEn,
                    f.Description,
                    CategoryId = f.CategoryId,
                    CategoryName = f.Category != null ? f.Category.NameFa : null,
                    f.CreatedAt
                })
                .ToListAsync();

            return Json(new { success = true, data = forms });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available forms");
            return Json(new { success = false, error = "خطا در دریافت فرم‌ها" });
        }
    }

    /// <summary>
    /// دریافت دسته‌بندی فرم‌ها
    /// </summary>
    [HttpGet]
    [Route("api/newdocument/categories")]
    public async Task<IActionResult> GetFormCategories()
    {
        try
        {
            var categories = await _context.FormCategories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.NameFa)
                .Select(c => new
                {
                    c.Id,
                    c.NameFa,
                    c.NameEn,
                    FormCount = c.Forms.Count(f => f.IsPublished && !f.IsDeleted)
                })
                .ToListAsync();

            return Json(new { success = true, data = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting form categories");
            return Json(new { success = false, error = "خطا در دریافت دسته‌بندی‌ها" });
        }
    }
}


