using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Automation.Web.Extensions;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت دبیرخانه
/// </summary>
public class ArchiveController : Controller
{
    private readonly IArchiveService _archiveService;
    private readonly ILogger<ArchiveController> _logger;
    private readonly AutomationDbContext _dbContext;

    public ArchiveController(IArchiveService archiveService, ILogger<ArchiveController> logger, AutomationDbContext dbContext)
    {
        _archiveService = archiveService;
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// صفحه دبیرخانه
    /// </summary>
    [HttpGet]
    [Route("archive")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// صفحه ثبت وارده
    /// </summary>
    [HttpGet]
    [Route("archive/incoming/register")]
    public IActionResult RegisterIncoming()
    {
        return View();
    }

    /// <summary>
    /// صفحه ثبت صادره
    /// </summary>
    [HttpGet]
    [Route("archive/outgoing/register")]
    public IActionResult RegisterOutgoing()
    {
        return View();
    }

    /// <summary>
    /// صفحه جستجوی دبیرخانه
    /// </summary>
    [HttpGet]
    [Route("archive/search")]
    public IActionResult Search()
    {
        return View();
    }

    /// <summary>
    /// ثبت نامه وارده
    /// </summary>
    [HttpPost]
    [Route("api/archive/incoming/register")]
    public async Task<IActionResult> RegisterIncomingDocument([FromBody] RegisterIncomingDto dto)
    {
        // اعتبارسنجی Model
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();
            
            return Json(new { success = false, errors = errors });
        }

        try
        {
            // دریافت UserId از Session یا Claims
            var userId = this.GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, error = "کاربر احراز هویت نشده است" });
            }
            
            // تنظیم RegisteredByUserId
            dto.RegisteredByUserId = userId;
            
            var result = await _archiveService.RegisterIncomingDocumentAsync(dto);
            
            if (!result.Success)
            {
                return Json(new { success = false, error = result.ErrorMessage });
            }
            
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering incoming document");
            return Json(new { success = false, error = "خطا در ثبت نامه وارده. لطفاً دوباره تلاش کنید." });
        }
    }

    /// <summary>
    /// ثبت نامه صادره
    /// </summary>
    [HttpPost]
    [Route("api/archive/outgoing/register")]
    public async Task<IActionResult> RegisterOutgoingDocument([FromBody] RegisterOutgoingDto dto)
    {
        // اعتبارسنجی Model
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();
            
            return Json(new { success = false, errors = errors });
        }

        try
        {
            // دریافت UserId از Session یا Claims
            var userId = this.GetCurrentUserId();
            if (userId == 0)
            {
                return Json(new { success = false, error = "کاربر احراز هویت نشده است" });
            }
            
            // تنظیم RegisteredByUserId
            dto.RegisteredByUserId = userId;
            
            var result = await _archiveService.RegisterOutgoingDocumentAsync(dto);
            
            if (!result.Success)
            {
                return Json(new { success = false, error = result.ErrorMessage });
            }
            
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering outgoing document");
            return Json(new { success = false, error = "خطا در ثبت نامه صادره. لطفاً دوباره تلاش کنید." });
        }
    }

    /// <summary>
    /// دریافت شماره وارده بعدی
    /// </summary>
    [HttpGet]
    [Route("api/archive/incoming/next-number")]
    public async Task<IActionResult> GetNextIncomingNumber([FromQuery] int year)
    {
        try
        {
            var number = await _archiveService.GetNextIncomingNumberAsync(year);
            return Json(new { success = true, number = number });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next incoming number");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت شماره صادره بعدی
    /// </summary>
    [HttpGet]
    [Route("api/archive/outgoing/next-number")]
    public async Task<IActionResult> GetNextOutgoingNumber([FromQuery] int year)
    {
        try
        {
            var number = await _archiveService.GetNextOutgoingNumberAsync(year);
            return Json(new { success = true, number = number });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next outgoing number");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست مدارک بایگانی شده
    /// </summary>
    [HttpGet]
    [Route("api/archive/documents")]
    public async Task<IActionResult> GetArchivedDocuments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var result = await _archiveService.GetArchivedDocumentsAsync(page, pageSize, search, fromDate, toDate);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting archived documents");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// بایگانی کردن مدرک
    /// </summary>
    [HttpPost]
    [Route("api/archive/documents/{documentId}/archive")]
    public async Task<IActionResult> ArchiveDocument(int documentId, [FromBody] ArchiveDocumentDto dto)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _archiveService.ArchiveDocumentAsync(documentId, userId, dto.ArchiveLocation);
            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving document {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست انواع مدرک
    /// </summary>
    [HttpGet]
    [Route("api/archive/documenttypes")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        try
        {
            var types = await _dbContext.DocumentTypes
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.DocumentTypeName)
                .Select(x => new { x.Id, x.DocumentTypeCode, x.DocumentTypeName, x.Description })
                .ToListAsync();
            
            return Json(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document types");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست طبقه‌بندی‌ها
    /// </summary>
    [HttpGet]
    [Route("api/archive/classifications")]
    public async Task<IActionResult> GetClassifications()
    {
        try
        {
            var classifications = await _dbContext.Classifications
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ClassificationName)
                .Select(x => new { x.Id, x.ClassificationCode, x.ClassificationName, x.Description })
                .ToListAsync();
            
            return Json(classifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting classifications");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست نحوه دریافت
    /// </summary>
    [HttpGet]
    [Route("api/archive/receiptmethods")]
    public async Task<IActionResult> GetReceiptMethods()
    {
        try
        {
            var methods = await _dbContext.ReceiptMethods
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ReceiptMethodName)
                .Select(x => new { x.Id, x.ReceiptMethodCode, x.ReceiptMethodName, x.Description })
                .ToListAsync();
            
            return Json(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt methods");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست فوریت‌ها
    /// </summary>
    [HttpGet]
    [Route("api/archive/urgencies")]
    public async Task<IActionResult> GetUrgencies()
    {
        try
        {
            var urgencies = await _dbContext.Urgencies
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.UrgencyName)
                .Select(x => new { x.Id, x.UrgencyCode, x.UrgencyName, x.Description })
                .ToListAsync();
            
            return Json(urgencies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting urgencies");
            return Json(new { success = false, error = ex.Message });
        }
    }
}





