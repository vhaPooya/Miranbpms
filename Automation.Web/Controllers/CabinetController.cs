using Automation.Core.Interfaces;
using Automation.Web.Extensions;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت کارتابل‌ها
/// </summary>
public class CabinetController : Controller
{
    private readonly ICabinetService _cabinetService;
    private readonly ILogger<CabinetController> _logger;

    public CabinetController(ICabinetService cabinetService, ILogger<CabinetController> logger)
    {
        _cabinetService = cabinetService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه کارتابل ورودی
    /// </summary>
    [HttpGet]
    [Route("cabinet/inbox")]
    public IActionResult Inbox()
    {
        return PartialView("_Inbox");
    }

    /// <summary>
    /// صفحه کارتابل ارجاعی
    /// </summary>
    [HttpGet]
    [Route("cabinet/referred")]
    public IActionResult Referred()
    {
        return PartialView("_Referred");
    }

    /// <summary>
    /// صفحه کارتابل صادره
    /// </summary>
    [HttpGet]
    [Route("cabinet/outbox")]
    public IActionResult Outbox()
    {
        return View();
    }

    /// <summary>
    /// صفحه کارتابل داخلی
    /// </summary>
    [HttpGet]
    [Route("cabinet/internal")]
    public IActionResult Internal()
    {
        return View();
    }

    /// <summary>
    /// صفحه کارتابل شخصی
    /// </summary>
    [HttpGet]
    [Route("cabinet/personal")]
    public IActionResult Personal()
    {
        return View();
    }

    /// <summary>
    /// دریافت کارتابل ورودی
    /// </summary>
    [HttpGet]
    [Route("api/cabinet/inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.GetInboxAsync(userId, page, pageSize, search, status);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inbox");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت کارتابل ارجاعی
    /// </summary>
    [HttpGet]
    [Route("api/cabinet/referred")]
    public async Task<IActionResult> GetReferred([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.GetReferredAsync(userId, page, pageSize, search, status);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting referred");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت کارتابل صادره
    /// </summary>
    [HttpGet]
    [Route("api/cabinet/outbox")]
    public async Task<IActionResult> GetOutbox([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.GetOutboxAsync(userId, page, pageSize, search);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting outbox");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت کارتابل داخلی
    /// </summary>
    [HttpGet]
    [Route("api/cabinet/internal")]
    public async Task<IActionResult> GetInternal([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.GetInternalAsync(userId, page, pageSize, search);
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting internal");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت جزئیات مدرک
    /// </summary>
    [HttpGet]
    [Route("api/cabinet/documents/{documentId}")]
    public async Task<IActionResult> GetDocumentDetails(int documentId)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.GetDocumentDetailsAsync(documentId, userId);
            if (result == null)
                return NotFound();
            
            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document details {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ثبت مشاهده
    /// </summary>
    [HttpPost]
    [Route("api/cabinet/documents/{documentId}/view")]
    public async Task<IActionResult> MarkAsViewed(int documentId)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.MarkAsViewedAsync(documentId, userId);
            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking document as viewed {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ثبت بررسی
    /// </summary>
    [HttpPost]
    [Route("api/cabinet/documents/{documentId}/review")]
    public async Task<IActionResult> MarkAsReviewed(int documentId, [FromBody] ReviewDto dto)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var result = await _cabinetService.MarkAsReviewedAsync(documentId, userId, dto.IsReviewed);
            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking document as reviewed {DocumentId}", documentId);
            return Json(new { success = false, error = ex.Message });
        }
    }
}





