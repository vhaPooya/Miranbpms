using Automation.Core.Interfaces;
using Automation.Web.Extensions;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت ایمیل
/// </summary>
public class EmailController : Controller
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه صندوق ایمیل
    /// </summary>
    [HttpGet]
    [Route("email")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// دریافت لیست ایمیل‌ها
    /// </summary>
    [HttpGet]
    [Route("api/email")]
    public async Task<IActionResult> GetEmails([FromQuery] int? settingsId = null, [FromQuery] string? messageType = null, [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var emails = await _emailService.GetEmailMessagesAsync(settingsId, messageType, status, page, pageSize);
            return Json(new { success = true, data = emails });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emails");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت ایمیل بر اساس شناسه
    /// </summary>
    [HttpGet]
    [Route("api/email/{id}")]
    public async Task<IActionResult> GetEmail(int id)
    {
        try
        {
            var email = await _emailService.GetEmailMessageAsync(id);
            if (email == null)
            {
                return Json(new { success = false, error = "ایمیل یافت نشد" });
            }
            return Json(new { success = true, data = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email {Id}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ارسال ایمیل
    /// </summary>
    [HttpPost]
    [Route("api/email/send")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
    {
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
            var senderUserId = this.GetCurrentUserId();
            var result = await _emailService.SendEmailAsync(
                dto.SettingsId,
                dto.ToEmail,
                dto.Subject,
                dto.Body,
                dto.IsHtml,
                dto.FromEmail,
                dto.FromName,
                dto.Cc,
                dto.Bcc,
                dto.Attachments,
                dto.DocumentId,
                senderUserId != 0 ? senderUserId : null
            );

            if (!result.Success)
            {
                return Json(new { success = false, error = result.ErrorMessage });
            }

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email");
            return Json(new { success = false, error = "خطا در ارسال ایمیل" });
        }
    }
}




