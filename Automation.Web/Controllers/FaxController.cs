using Automation.Core.Interfaces;
using Automation.Web.Extensions;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت فکس
/// </summary>
public class FaxController : Controller
{
    private readonly IFaxService _faxService;
    private readonly ILogger<FaxController> _logger;

    public FaxController(IFaxService faxService, ILogger<FaxController> logger)
    {
        _faxService = faxService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه صندوق فکس
    /// </summary>
    [HttpGet]
    [Route("fax")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// دریافت لیست فکس‌ها
    /// </summary>
    [HttpGet]
    [Route("api/fax")]
    public async Task<IActionResult> GetFaxes([FromQuery] int? settingsId, [FromQuery] string? messageType, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var faxes = await _faxService.GetFaxMessagesAsync(settingsId, messageType, page, pageSize);
            return Json(new { success = true, data = faxes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting faxes");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت فکس بر اساس شناسه
    /// </summary>
    [HttpGet]
    [Route("api/fax/{id}")]
    public async Task<IActionResult> GetFax(int id)
    {
        try
        {
            var fax = await _faxService.GetFaxMessageAsync(id);
            if (fax == null)
            {
                return Json(new { success = false, error = "فکس یافت نشد" });
            }
            return Json(new { success = true, data = fax });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fax {Id}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ارسال فکس
    /// </summary>
    [HttpPost]
    [Route("api/fax/send")]
    public async Task<IActionResult> SendFax([FromForm] SendFaxDto dto)
    {
        try
        {
            var senderUserId = this.GetCurrentUserId();
            
            // ذخیره فایل موقت
            var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp", $"fax_{Guid.NewGuid()}_{dto.File.FileName}");
            var tempDir = Path.GetDirectoryName(tempPath);
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }
            
            var result = await _faxService.SendFaxAsync(
                dto.SettingsId,
                dto.ToFaxNumber,
                tempPath,
                null,
                null,
                dto.ToName,
                null,
                senderUserId != 0 ? senderUserId : null
            );
            
            if (result.Success)
            {
                return Json(new { success = true, data = new { faxMessageId = result.FaxMessageId } });
            }
            
            return Json(new { success = false, error = result.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending fax");
            return Json(new { success = false, error = ex.Message });
        }
    }
}




