using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Services;

/// <summary>
/// سرویس اعتبارسنجی مدارک
/// </summary>
public class DocumentValidationService
{
    private readonly AutomationDbContext _context;

    public DocumentValidationService(AutomationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// اعتبارسنجی تاریخ مدرک (نباید از تاریخ وارده/صادره بیشتر باشد)
    /// </summary>
    public bool ValidateDocumentDate(DateTime? documentDate, DateTime? incomingDate, DateTime? outgoingDate)
    {
        if (!documentDate.HasValue)
            return true; // اختیاری است

        var referenceDate = incomingDate ?? outgoingDate ?? DateTime.UtcNow;
        
        // تاریخ مدرک نباید از تاریخ وارده/صادره بیشتر باشد
        return documentDate.Value <= referenceDate;
    }

    /// <summary>
    /// اعتبارسنجی یکتایی شماره وارده
    /// </summary>
    public async Task<bool> IsIncomingNumberUniqueAsync(string incomingNumber, int? excludeDocumentId = null)
    {
        if (string.IsNullOrEmpty(incomingNumber))
            return true;

        var query = _context.IncomingDocuments
            .Where(id => id.IncomingNumber == incomingNumber && !id.IsDeleted);

        if (excludeDocumentId.HasValue)
        {
            var document = await _context.IncomingDocuments
                .Where(id => id.Id == excludeDocumentId.Value)
                .Select(id => id.DocumentId)
                .FirstOrDefaultAsync();
            
            if (document != 0)
            {
                query = query.Where(id => id.DocumentId != document);
            }
        }

        return !await query.AnyAsync();
    }

    /// <summary>
    /// اعتبارسنجی یکتایی شماره صادره
    /// </summary>
    public async Task<bool> IsOutgoingNumberUniqueAsync(string outgoingNumber, int? excludeDocumentId = null)
    {
        if (string.IsNullOrEmpty(outgoingNumber))
            return true;

        var query = _context.OutgoingDocuments
            .Where(od => od.OutgoingNumber == outgoingNumber && !od.IsDeleted);

        if (excludeDocumentId.HasValue)
        {
            var document = await _context.OutgoingDocuments
                .Where(od => od.Id == excludeDocumentId.Value)
                .Select(od => od.DocumentId)
                .FirstOrDefaultAsync();
            
            if (document != 0)
            {
                query = query.Where(od => od.DocumentId != document);
            }
        }

        return !await query.AnyAsync();
    }

    /// <summary>
    /// اعتبارسنجی کامل DTO وارده
    /// </summary>
    public async Task<ValidationResult> ValidateIncomingDocumentAsync(RegisterIncomingDto dto)
    {
        var errors = new List<string>();

        // اعتبارسنجی تاریخ
        if (!ValidateDocumentDate(dto.DocumentDate, dto.IncomingDate, null))
        {
            errors.Add("تاریخ مدرک نمی‌تواند از تاریخ وارده بیشتر باشد");
        }

        // اعتبارسنجی یکتایی شماره
        if (!string.IsNullOrEmpty(dto.IncomingNumber))
        {
            if (!await IsIncomingNumberUniqueAsync(dto.IncomingNumber))
            {
                errors.Add($"شماره وارده {dto.IncomingNumber} قبلاً استفاده شده است");
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    /// <summary>
    /// اعتبارسنجی کامل DTO صادره
    /// </summary>
    public async Task<ValidationResult> ValidateOutgoingDocumentAsync(RegisterOutgoingDto dto)
    {
        var errors = new List<string>();

        // اعتبارسنجی تاریخ
        if (!ValidateDocumentDate(dto.DocumentDate, null, dto.OutgoingDate))
        {
            errors.Add("تاریخ مدرک نمی‌تواند از تاریخ صادره بیشتر باشد");
        }

        // اعتبارسنجی یکتایی شماره
        if (!string.IsNullOrEmpty(dto.OutgoingNumber))
        {
            if (!await IsOutgoingNumberUniqueAsync(dto.OutgoingNumber))
            {
                errors.Add($"شماره صادره {dto.OutgoingNumber} قبلاً استفاده شده است");
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// نتیجه اعتبارسنجی
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}




