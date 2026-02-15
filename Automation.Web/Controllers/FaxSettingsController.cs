using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت تنظیمات فکس
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class FaxSettingsController : ControllerBase
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<FaxSettingsController> _logger;

    public FaxSettingsController(AutomationDbContext context, ILogger<FaxSettingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست تنظیمات فکس
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFaxSettings()
    {
        try
        {
            var settings = await _context.Set<FaxServiceSettings>()
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.IsDefault ? 0 : 1)
                .ThenBy(s => s.Description)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.GatewayType,
                    s.GatewayUrl,
                    s.Username,
                    s.ApiKey,
                    SenderFaxNumber = s.DefaultFaxNumber,
                    DefaultFaxNumber = s.DefaultFaxNumber,
                    s.IsDefault,
                    s.Description,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(new { success = true, data = settings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fax settings");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت تنظیمات فکس بر اساس شناسه
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFaxSetting(int id)
    {
        try
        {
            var setting = await _context.Set<FaxServiceSettings>()
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.GatewayType,
                    s.GatewayUrl,
                    s.Username,
                    s.ApiKey,
                    SenderFaxNumber = s.DefaultFaxNumber,
                    DefaultFaxNumber = s.DefaultFaxNumber,
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
            _logger.LogError(ex, "Error getting fax setting {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ایجاد تنظیمات فکس جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateFaxSetting([FromBody] CreateFaxSettingDto dto)
    {
        try
        {
            // اگر به عنوان پیش‌فرض تنظیم شود، بقیه را غیرفعال می‌کنیم
            if (dto.IsDefault)
            {
                var existingDefaults = await _context.Set<FaxServiceSettings>()
                    .Where(s => s.IsDefault && !s.IsDeleted)
                    .ToListAsync();
                
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                }
            }

            var setting = new FaxServiceSettings
            {
                Name = dto.Name ?? dto.Description ?? "تنظیمات جدید",
                GatewayType = dto.GatewayType ?? "API",
                GatewayUrl = dto.GatewayUrl,
                Username = dto.Username,
                Password = dto.Password, // باید رمزنگاری شود
                ApiKey = dto.ApiKey,
                DefaultFaxNumber = dto.SenderFaxNumber,
                IsDefault = dto.IsDefault,
                Description = dto.Description,
                IsActive = dto.IsActive ?? true
            };

            _context.Set<FaxServiceSettings>().Add(setting);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Fax setting created: {Id}", setting.Id);

            return Ok(new { success = true, data = new { setting.Id } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fax setting");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// به‌روزرسانی تنظیمات فکس
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFaxSetting(int id, [FromBody] UpdateFaxSettingDto dto)
    {
        try
        {
            var setting = await _context.Set<FaxServiceSettings>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (setting == null)
            {
                return NotFound(new { success = false, error = "تنظیمات یافت نشد" });
            }

            // اگر به عنوان پیش‌فرض تنظیم شود، بقیه را غیرفعال می‌کنیم
            if (dto.IsDefault && !setting.IsDefault)
            {
                var existingDefaults = await _context.Set<FaxServiceSettings>()
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
            if (!string.IsNullOrEmpty(dto.GatewayType))
            {
                setting.GatewayType = dto.GatewayType;
            }
            setting.GatewayUrl = dto.GatewayUrl;
            setting.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                setting.Password = dto.Password; // باید رمزنگاری شود
            }
            if (!string.IsNullOrEmpty(dto.ApiKey))
            {
                setting.ApiKey = dto.ApiKey;
            }
            setting.DefaultFaxNumber = dto.SenderFaxNumber;
            setting.IsDefault = dto.IsDefault;
            setting.Description = dto.Description;
            if (dto.IsActive.HasValue)
            {
                setting.IsActive = dto.IsActive.Value;
            }
            setting.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Fax setting updated: {Id}", id);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fax setting {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// حذف تنظیمات فکس
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFaxSetting(int id)
    {
        try
        {
            var setting = await _context.Set<FaxServiceSettings>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (setting == null)
            {
                return NotFound(new { success = false, error = "تنظیمات یافت نشد" });
            }

            setting.IsDeleted = true;
            setting.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Fax setting deleted: {Id}", id);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fax setting {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}




