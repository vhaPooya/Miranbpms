using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت کارتابل‌ها
/// </summary>
public class CabinetService : ICabinetService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<CabinetService> _logger;

    public CabinetService(AutomationDbContext context, ILogger<CabinetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CabinetListDto> GetInboxAsync(int userId, int page = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        var query = _context.Documents
            .Include(d => d.CreatedByUser)
            .Where(d => !d.IsDeleted && d.CreatedByUserId == userId);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Subject.Contains(search) || d.DocumentNumber.Contains(search));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new CabinetItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                Status = d.Status,
                CreatedDateTime = d.CreatedDateTime,
                IsViewed = false,
                IsReviewed = false,
                IsUnderReview = false,
                AttachmentCount = d.Attachments.Count
            })
            .ToListAsync();

        return new CabinetListDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CabinetListDto> GetReferredAsync(int userId, int page = 1, int pageSize = 10, string? search = null, string? status = null)
    {
        var query = _context.DocumentReferrals
            .Include(dr => dr.Document)
            .Include(dr => dr.ActionType)
            .Where(dr => dr.ReferredToUserId == userId && !dr.Document.IsDeleted);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(dr => dr.Document.Subject.Contains(search) || dr.Document.DocumentNumber.Contains(search));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(dr => dr.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(dr => dr.ReferredDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(dr => new CabinetItemDto
            {
                DocumentId = dr.DocumentId,
                DocumentNumber = dr.Document.DocumentNumber,
                Subject = dr.Document.Subject,
                DocumentType = dr.Document.DocumentType,
                Status = dr.Status,
                CreatedDateTime = dr.Document.CreatedDateTime,
                ReceivedDateTime = dr.ReferredDate,
                FirstViewedDateTime = dr.ReadDate,
                IsViewed = dr.IsRead,
                IsReviewed = false,
                IsUnderReview = dr.Status == "UNDER_REVIEW",
                ActionType = dr.ActionType.ActionNameFa ?? string.Empty,
                AttachmentCount = dr.Document.Attachments.Count
            })
            .ToListAsync();

        return new CabinetListDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CabinetListDto> GetOutboxAsync(int userId, int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Documents
            .Include(d => d.CreatedByUser)
            .Where(d => !d.IsDeleted && d.CreatedByUserId == userId && d.DocumentType == "OUTGOING");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Subject.Contains(search) || d.DocumentNumber.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new CabinetItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                Status = d.Status,
                CreatedDateTime = d.CreatedDateTime,
                IsViewed = false,
                IsReviewed = false,
                IsUnderReview = false,
                AttachmentCount = d.Attachments.Count
            })
            .ToListAsync();

        return new CabinetListDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CabinetListDto> GetInternalAsync(int userId, int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Documents
            .Include(d => d.CreatedByUser)
            .Where(d => !d.IsDeleted && d.DocumentType == "INTERNAL");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Subject.Contains(search) || d.DocumentNumber.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new CabinetItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                Status = d.Status,
                CreatedDateTime = d.CreatedDateTime,
                IsViewed = false,
                IsReviewed = false,
                IsUnderReview = false,
                AttachmentCount = d.Attachments.Count
            })
            .ToListAsync();

        return new CabinetListDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CabinetListDto> GetPersonalAsync(int userId, int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Documents
            .Include(d => d.CreatedByUser)
            .Where(d => !d.IsDeleted && d.CreatedByUserId == userId && d.DocumentType == "PERSONAL");

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Subject.Contains(search) || d.DocumentNumber.Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new CabinetItemDto
            {
                DocumentId = d.Id,
                DocumentNumber = d.DocumentNumber,
                Subject = d.Subject,
                DocumentType = d.DocumentType,
                Status = d.Status,
                CreatedDateTime = d.CreatedDateTime,
                IsViewed = false,
                IsReviewed = false,
                IsUnderReview = false,
                AttachmentCount = d.Attachments.Count
            })
            .ToListAsync();

        return new CabinetListDto
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> MarkAsViewedAsync(int documentId, int userId)
    {
        var referral = await _context.DocumentReferrals
            .FirstOrDefaultAsync(dr => dr.DocumentId == documentId && dr.ReferredToUserId == userId);

        if (referral != null && !referral.IsRead)
        {
            referral.IsRead = true;
            referral.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> MarkAsReviewedAsync(int documentId, int userId, bool isReviewed)
    {
        // Implementation for marking document as reviewed
        // This would typically update a tracking table
        return await Task.FromResult(true);
    }

    public async Task<CabinetDocumentDto?> GetDocumentDetailsAsync(int documentId, int userId)
    {
        var document = await _context.Documents
            .Include(d => d.Attachments)
            .Include(d => d.Referrals)
                .ThenInclude(r => r.ActionType)
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            return null;

        return new CabinetDocumentDto
        {
            DocumentId = document.Id,
            DocumentNumber = document.DocumentNumber,
            Subject = document.Subject,
            Content = document.Content ?? string.Empty,
            Status = document.Status,
            IsSigned = document.IsSigned,
            CanEdit = document.CreatedByUserId == userId,
            Attachments = document.Attachments.Select(a => new DocumentAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName ?? string.Empty,
                AttachmentType = a.AttachmentType ?? string.Empty,
                FileSize = a.FileSize,
                UploadedDate = a.UploadedDate
            }).ToList(),
            Referrals = document.Referrals.Select(r => new DocumentReferralDto
            {
                Id = r.Id,
                ActionTypeName = r.ActionType.ActionNameFa ?? string.Empty,
                ReferredToName = string.Empty, // Would need to join with User table
                ReferredDate = r.ReferredDate,
                Status = r.Status,
                Notes = r.Notes
            }).ToList()
        };
    }
}



