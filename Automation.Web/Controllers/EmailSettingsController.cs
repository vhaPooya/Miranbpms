using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت تنظیمات ایمیل
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class EmailSettingsController : ControllerBase
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<EmailSettingsController> _logger;

    public EmailSettingsController(AutomationDbContext context, ILogger<EmailSettingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست تنظیمات ایمیل
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEmailSettings()
    {
        try
        {
            var settings = await _context.Set<EmailServiceSettings>()
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.IsDefault ? 0 : 1)
                .ThenBy(s => s.Description)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.SmtpServer,
                    s.SmtpPort,
                    s.Username,
                    FromAddress = s.FromEmail,
                    FromEmail = s.FromEmail,
                    s.FromName,
                    EnableSsl = s.UseSsl,
                    UseSsl = s.UseSsl,
                    s.IsDefault,
                    s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new { success = true, data = settings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email settings");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت تنظیمات ایمیل بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmailSetting(int id)
    {
        try
        {
            var setting = await _context.Set<EmailServiceSettings>()
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.SmtpServer,
                    s.SmtpPort,
                    s.Username,
                    FromAddress = s.FromEmail,
                    FromEmail = s.FromEmail,
                    s.FromName,
                    EnableSsl = s.UseSsl,
                    UseSsl = s.UseSsl,
                    s.IsDefault,
                    s.Description,
                    IsActive = s.IsActive
                })
                .FirstOrDefaultAsync();

            if (setting == null)
            {
                return NotFound(new { success = false, error = "تنظیمات یافت نشد" });
            }

            return Ok(new { success = true, data = setting });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email setting {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ایجاد تنظیمات ایمیل جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEmailSetting([FromBody] CreateEmailSettingDto dto)
    {
        try
        {
            // اگر به عنوان پیش‌فرض تنظیم شود، بقیه را غیرفعال می‌کنیم
            if (dto.IsDefault)
            {
                var existingDefaults = await _context.Set<EmailServiceSettings>()
                    .Where(s => s.IsDefault && !s.IsDeleted)
                    .ToListAsync();
                
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }
            }

            var setting = new EmailServiceSettings
            {
                Name = dto.Name ?? dto.Description ?? "تنظیمات جدید",
                SmtpServer = dto.SmtpServer,
                SmtpPort = dto.SmtpPort,
                Username = dto.Username,
                Password = dto.Password, // باید رمزنگاری شود
                FromEmail = dto.FromAddress,
                FromName = dto.FromName,
                UseSsl = dto.EnableSsl,
                IsDefault = dto.IsDefault,
                Description = dto.Description,
                IsActive = dto.IsActive ?? true
            };

            _context.Set<EmailServiceSettings>().Add(setting);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email setting created: {Id}", setting.Id);

            return Ok(new { success = true, data = new { setting.Id } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email setting");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// به‌روزرسانی تنظیمات ایمیل
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmailSetting(int id, [FromBody] UpdateEmailSettingDto dto)
    {
        try
        {
            var setting = await _context.Set<EmailServiceSettings>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (setting == null)
            {
                return NotFound(new { success = false, error = "تنظیمات یافت نشد" });
            }

            // اگر به عنوان پیش‌فرض تنظیم شود، بقیه را غیرفعال می‌کنیم
            if (dto.IsDefault && !setting.IsDefault)
            {
                var existingDefaults = await _context.Set<EmailServiceSettings>()
                    .Where(s => s.IsDefault && s.Id != id && !s.IsDeleted)
                    .ToListAsync();
                
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }
            }

            if (!string.IsNullOrEmpty(dto.Name))
            {
                setting.Name = dto.Name;
            }
            setting.SmtpServer = dto.SmtpServer;
            setting.SmtpPort = dto.SmtpPort;
            setting.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                setting.Password = dto.Password; // باید رمزنگاری شود
            }
            setting.FromEmail = dto.FromAddress;
            setting.FromName = dto.FromName;
            setting.UseSsl = dto.EnableSsl;
            setting.IsDefault = dto.IsDefault;
            setting.Description = dto.Description;
            if (dto.IsActive.HasValue)
            {
                setting.IsActive = dto.IsActive.Value;
            }
            setting.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email setting updated: {Id}", id);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email setting {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// حذف تنظیمات ایمیل
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmailSetting(int id)
    {
        try
        {
            var setting = await _context.Set<EmailServiceSettings>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (setting == null)
            {
                return NotFound(new { success = false, error = "تنظیمات یافت نشد" });
            }

            setting.IsDeleted = true;
            setting.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email setting deleted: {Id}", id);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email setting {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}




