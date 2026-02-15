using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Web.Extensions;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت شماره‌های رزرو شده
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ReservedNumberController : Controller
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<ReservedNumberController> _logger;

    public ReservedNumberController(AutomationDbContext context, ILogger<ReservedNumberController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// دریافت لیست شماره‌های رزرو شده
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReservedNumbers([FromQuery] int? secretariatId, [FromQuery] string? documentType, [FromQuery] bool? isUsed)
    {
        try
        {
            var query = _context.Set<ReservedDocumentNumber>()
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            if (secretariatId.HasValue)
            {
                query = query.Where(r => r.SecretariatId == secretariatId.Value);
            }

            if (!string.IsNullOrEmpty(documentType))
            {
                query = query.Where(r => r.DocumentType == documentType);
            }

            if (isUsed.HasValue)
            {
                query = query.Where(r => r.IsUsed == isUsed.Value);
            }

            var reservedNumbers = await query
                .OrderByDescending(r => r.ReservedDate)
                .Select(r => new
                {
                    r.Id,
                    r.ReservedNumber,
                    r.DocumentType,
                    r.SecretariatId,
                    SecretariatName = r.Secretariat.SecretariatName,
                    r.ReservedByUserId,
                    ReservedByUserName = r.ReservedByUser.FirstName + " " + r.ReservedByUser.LastName,
                    r.ReservedDate,
                    r.ExpiryDate,
                    r.IsUsed,
                    r.UsedByDocumentId,
                    r.Description
                })
                .ToListAsync();

            return Ok(new { success = true, data = reservedNumbers });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reserved numbers");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// رزرو شماره جدید
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReserveNumber([FromBody] ReserveNumberDto dto)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { success = false, error = "کاربر احراز هویت نشده است" });
            }

            // بررسی تکراری نبودن شماره
            var existing = await _context.Set<ReservedDocumentNumber>()
                .FirstOrDefaultAsync(r => r.ReservedNumber == dto.ReservedNumber && 
                                          r.SecretariatId == dto.SecretariatId && 
                                          r.DocumentType == dto.DocumentType && 
                                          !r.IsUsed && 
                                          !r.IsDeleted);

            if (existing != null)
            {
                return BadRequest(new { success = false, error = "این شماره قبلاً رزرو شده است" });
            }

            var reservedNumber = new ReservedDocumentNumber
            {
                ReservedNumber = dto.ReservedNumber,
                DocumentType = dto.DocumentType,
                SecretariatId = dto.SecretariatId,
                ReservedByUserId = userId,
                ReservedDate = DateTime.UtcNow,
                ExpiryDate = dto.ExpiryDate,
                Description = dto.Description
            };

            _context.Set<ReservedDocumentNumber>().Add(reservedNumber);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Number reserved: {ReservedNumber} by user {UserId}", dto.ReservedNumber, userId);

            return Ok(new { success = true, data = new { reservedNumber.Id } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving number");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// حذف رزرو شماره
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservedNumber(int id)
    {
        try
        {
            var reservedNumber = await _context.Set<ReservedDocumentNumber>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (reservedNumber == null)
            {
                return NotFound(new { success = false, error = "شماره رزرو شده یافت نشد" });
            }

            if (reservedNumber.IsUsed)
            {
                return BadRequest(new { success = false, error = "شماره استفاده شده قابل حذف نیست" });
            }

            reservedNumber.IsDeleted = true;
            reservedNumber.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reserved number deleted: {Id}", id);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reserved number {Id}", id);
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}





