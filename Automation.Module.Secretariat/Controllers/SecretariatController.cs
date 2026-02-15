using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Module.Secretariat.Controllers;

/// <summary>
/// کنترلر مدیریت دبیرخانه‌ها — از CoreDbContext برای دیتای دبیرخانه و AutomationDbContext برای بررسی مدارک استفاده می‌کند.
/// </summary>
public class SecretariatController : Controller
{
    private readonly CoreDbContext _coreContext;
    private readonly AutomationDbContext _context;
    private readonly ILogger<SecretariatController> _logger;
    private readonly ISessionService _sessionService;

    public SecretariatController(CoreDbContext coreContext, AutomationDbContext context, ILogger<SecretariatController> logger, ISessionService sessionService)
    {
        _coreContext = coreContext;
        _context = context;
        _logger = logger;
        _sessionService = sessionService;
    }

    /// <summary>
    /// صفحه اصلی دبیرخانه
    /// </summary>
    [HttpGet]
    [Route("secretariat")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    /// <summary>
    /// انتخاب دبیرخانه جاری
    /// </summary>
    [HttpPost]
    [Route("api/secretariat/select/{id}")]
    public IActionResult SelectSecretariat(int id)
    {
        try
        {
            _sessionService.SetCurrentSecretariatId(id);
            return Json(new { success = true, message = "دبیرخانه با موفقیت انتخاب شد" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting secretariat {Id}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت دبیرخانه جاری
    /// </summary>
    [HttpGet]
    [Route("api/secretariat/getcurrent")]
    public async Task<IActionResult> GetCurrentSecretariat()
    {
        try
        {
            var secretariatId = _sessionService.GetCurrentSecretariatId();
            if (!secretariatId.HasValue)
            {
                return Json(new { success = false, message = "هیچ دبیرخانه‌ای انتخاب نشده است" });
            }

            var secretariat = await _coreContext.Secretariats
                .Include(s => s.Organization)
                .Where(s => s.Id == secretariatId.Value && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.SecretariatCode,
                    s.SecretariatName,
                    s.OrganizationId,
                    OrganizationName = s.Organization.OrganizationName,
                    s.EmailAddress,
                    s.FaxNumber,
                    s.IsActive
                })
                .FirstOrDefaultAsync();

            if (secretariat == null)
            {
                _sessionService.SetCurrentSecretariatId(null);
                return Json(new { success = false, message = "دبیرخانه انتخاب شده یافت نشد" });
            }

            return Json(new { success = true, data = secretariat });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current secretariat");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت لیست دبیرخانه‌ها
    /// </summary>
    [HttpGet]
    [Route("api/secretariat")]
    public async Task<IActionResult> GetSecretariats([FromQuery] int? organizationId = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var query = _coreContext.Secretariats
                .Include(s => s.Organization)
                .Where(s => !s.IsDeleted);

            if (organizationId.HasValue)
            {
                query = query.Where(s => s.OrganizationId == organizationId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            var secretariats = await query
                .OrderBy(s => s.SecretariatName)
                .Select(s => new
                {
                    s.Id,
                    s.SecretariatCode,
                    s.SecretariatName,
                    s.OrganizationId,
                    OrganizationName = s.Organization.OrganizationName,
                    s.EmailAddress,
                    s.FaxNumber,
                    s.IsActive,
                    s.Description
                })
                .ToListAsync();

            return Json(new { success = true, data = secretariats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secretariats");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت دبیرخانه بر اساس شناسه
    /// </summary>
    [HttpGet]
    [Route("api/secretariat/{id}")]
    public async Task<IActionResult> GetSecretariat(int id)
    {
        try
        {
            var secretariat = await _coreContext.Secretariats
                .Include(s => s.Organization)
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new
                {
                    s.Id,
                    s.SecretariatCode,
                    s.SecretariatName,
                    s.OrganizationId,
                    OrganizationName = s.Organization.OrganizationName,
                    s.EmailAddress,
                    s.FaxNumber,
                    s.IsActive,
                    s.Description
                })
                .FirstOrDefaultAsync();

            if (secretariat == null)
            {
                return Json(new { success = false, error = "دبیرخانه یافت نشد" });
            }

            return Json(new { success = true, data = secretariat });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secretariat {Id}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ایجاد دبیرخانه جدید
    /// </summary>
    [HttpPost]
    [Route("api/secretariat")]
    public async Task<IActionResult> CreateSecretariat([FromBody] CreateSecretariatDto dto)
    {
        try
        {
            var exists = await _coreContext.Secretariats
                .AnyAsync(s => s.SecretariatCode == dto.SecretariatCode && !s.IsDeleted);

            if (exists)
            {
                return Json(new { success = false, error = "کد دبیرخانه تکراری است" });
            }

            var secretariat = new Automation.Core.Entities.Secretariat
            {
                SecretariatCode = dto.SecretariatCode,
                SecretariatName = dto.SecretariatName,
                OrganizationId = dto.OrganizationId,
                EmailAddress = dto.EmailAddress,
                FaxNumber = dto.FaxNumber,
                IsActive = dto.IsActive ?? true,
                Description = dto.Description
            };

            _coreContext.Secretariats.Add(secretariat);
            await _coreContext.SaveChangesAsync();

            _logger.LogInformation("Secretariat created: {SecretariatId}", secretariat.Id);

            return Json(new { success = true, data = new { secretariat.Id } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating secretariat");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// به‌روزرسانی دبیرخانه
    /// </summary>
    [HttpPut]
    [Route("api/secretariat/{id}")]
    public async Task<IActionResult> UpdateSecretariat(int id, [FromBody] UpdateSecretariatDto dto)
    {
        try
        {
            var secretariat = await _coreContext.Secretariats
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (secretariat == null)
            {
                return Json(new { success = false, error = "دبیرخانه یافت نشد" });
            }

            if (secretariat.SecretariatCode != dto.SecretariatCode)
            {
                var exists = await _coreContext.Secretariats
                    .AnyAsync(s => s.SecretariatCode == dto.SecretariatCode && s.Id != id && !s.IsDeleted);

                if (exists)
                {
                    return Json(new { success = false, error = "کد دبیرخانه تکراری است" });
                }
            }

            secretariat.SecretariatCode = dto.SecretariatCode;
            secretariat.SecretariatName = dto.SecretariatName;
            secretariat.OrganizationId = dto.OrganizationId;
            secretariat.EmailAddress = dto.EmailAddress;
            secretariat.FaxNumber = dto.FaxNumber;
            secretariat.IsActive = dto.IsActive ?? secretariat.IsActive;
            secretariat.Description = dto.Description;
            secretariat.EditDate = DateTime.UtcNow;

            await _coreContext.SaveChangesAsync();

            _logger.LogInformation("Secretariat updated: {SecretariatId}", id);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating secretariat {Id}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// حذف دبیرخانه
    /// </summary>
    [HttpDelete]
    [Route("api/secretariat/{id}")]
    public async Task<IActionResult> DeleteSecretariat(int id)
    {
        try
        {
            var secretariat = await _coreContext.Secretariats
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (secretariat == null)
            {
                return Json(new { success = false, error = "دبیرخانه یافت نشد" });
            }

            var hasDocuments = await _context.IncomingDocuments
                .AnyAsync(d => d.SecretariatId == id && !d.IsDeleted) ||
                await _context.OutgoingDocuments
                .AnyAsync(d => d.SecretariatId == id && !d.IsDeleted);

            if (hasDocuments)
            {
                return Json(new { success = false, error = "نمی‌توان دبیرخانه‌ای که در مدارک استفاده شده است را حذف کرد" });
            }

            secretariat.IsDeleted = true;
            secretariat.EditDate = DateTime.UtcNow;

            await _coreContext.SaveChangesAsync();

            _logger.LogInformation("Secretariat deleted: {SecretariatId}", id);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting secretariat {Id}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }
}
