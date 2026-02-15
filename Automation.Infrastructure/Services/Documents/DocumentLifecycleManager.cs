using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Automation.Infrastructure.Services.Documents;

/// <summary>
/// مدیریت چرخه عمر اسناد
/// </summary>
public class DocumentLifecycleManager
{
    private readonly AutomationDbContext _context;

    public DocumentLifecycleManager(AutomationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ایجاد سند جدید
    /// </summary>
    public async Task<Document> CreateDocumentAsync(DocumentCreationRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ایجاد سند اصلی
            var document = new Document
            {
                DocumentTypeId = request.DocumentTypeId,
                DocumentNumber = await GenerateDocumentNumberAsync(request.DocumentTypeId, request.SecretariatId),
                Subject = request.Subject,
                Content = request.Content,
                Status = "DRAFT",
                PriorityCode = request.Priority,
                Classification = request.Classification,
                CreatedById = request.CreatedById,
                SecretariatId = request.SecretariatId,
                CreationDate = DateTime.UtcNow,
                DueDate = request.DueDate
            };

            _context.Set<Document>().Add(document);
            await _context.SaveChangesAsync();

            // ایجاد ردیابی اولیه
            var tracking = new DocumentTracking
            {
                DocumentId = document.Id,
                UserId = request.CreatedById,
                Status = "CREATED",
                ActionPerformed = "ایجاد سند",
                PerformedDate = DateTime.UtcNow
            };

            _context.Set<DocumentTracking>().Add(tracking);

            // اضافه کردن گیرندگان
            foreach (var recipient in request.Recipients)
            {
                var distribution = new DocumentDistribution
                {
                    DocumentId = document.Id,
                    RecipientUserId = recipient.UserId,
                    RecipientDepartmentId = recipient.DepartmentId,
                    DistributionMethod = recipient.DistributionMethod,
                    Status = "PENDING",
                    DistributedDate = DateTime.UtcNow
                };

                _context.Set<DocumentDistribution>().Add(distribution);
            }

            // اضافه کردن پیوست‌ها
            foreach (var attachment in request.Attachments)
            {
                var docAttachment = new DocumentAttachment
                {
                    DocumentId = document.Id,
                    FileName = attachment.FileName,
                    FileSize = attachment.FileSize,
                    ContentType = attachment.ContentType,
                    StoragePath = attachment.StoragePath,
                    Description = attachment.Description,
                    UploadedById = request.CreatedById,
                    UploadDate = DateTime.UtcNow
                };

                _context.Set<DocumentAttachment>().Add(docAttachment);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return document;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// ارسال سند برای بازنگری
    /// </summary>
    public async Task<bool> SubmitForReviewAsync(int documentId, int reviewerId, string notes = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var document = await _context.Set<Document>()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null || document.Status != "DRAFT")
                return false;

            // تغییر وضعیت سند
            document.Status = "UNDER_REVIEW";
            document.ModifiedDate = DateTime.UtcNow;

            // ایجاد ردیابی
            var tracking = new DocumentTracking
            {
                DocumentId = documentId,
                UserId = reviewerId,
                Status = "REVIEW_REQUESTED",
                ActionPerformed = "درخواست بازنگری",
                PerformedDate = DateTime.UtcNow,
                Notes = notes
            };

            _context.Set<DocumentTracking>().Add(tracking);

            // ایجاد تخصیص برای بازنگر
            var assignment = new DocumentAssignment
            {
                DocumentId = documentId,
                AssignedToUserId = reviewerId,
                AssignmentType = "REVIEW",
                AssignedDate = DateTime.UtcNow,
                Status = "PENDING",
                Notes = notes
            };

            _context.Set<DocumentAssignment>().Add(assignment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // ایجاد اعلان
            await CreateNotificationAsync(reviewerId, "درخواست بازنگری سند", $"سند '{document.Subject}' برای بازنگری به شما اختصاص داده شده است.");

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// تایید سند
    /// </summary>
    public async Task<bool> ApproveDocumentAsync(int documentId, int approverId, string notes = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var document = await _context.Set<Document>()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            // تغییر وضعیت سند
            document.Status = "APPROVED";
            document.ApprovedById = approverId;
            document.ApprovedDate = DateTime.UtcNow;
            document.ModifiedDate = DateTime.UtcNow;

            // ایجاد ردیابی
            var tracking = new DocumentTracking
            {
                DocumentId = documentId,
                UserId = approverId,
                Status = "APPROVED",
                ActionPerformed = "تایید سند",
                PerformedDate = DateTime.UtcNow,
                Notes = notes
            };

            _context.Set<DocumentTracking>().Add(tracking);

            // به‌روزرسانی تخصیص‌های مرتبط
            var assignments = await _context.Set<DocumentAssignment>()
                .Where(da => da.DocumentId == documentId && da.Status == "PENDING")
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.Status = "COMPLETED";
                assignment.CompletedDate = DateTime.UtcNow;
                assignment.Notes = notes;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // ایجاد اعلان برای فرستنده
            await CreateNotificationAsync(document.CreatedById, "سند تایید شد", $"سند '{document.Subject}' توسط شما تایید شد.");

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// رد سند
    /// </summary>
    public async Task<bool> RejectDocumentAsync(int documentId, int rejectorId, string reason)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var document = await _context.Set<Document>()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            // تغییر وضعیت سند
            document.Status = "REJECTED";
            document.ModifiedDate = DateTime.UtcNow;

            // ایجاد ردیابی
            var tracking = new DocumentTracking
            {
                DocumentId = documentId,
                UserId = rejectorId,
                Status = "REJECTED",
                ActionPerformed = "رد سند",
                PerformedDate = DateTime.UtcNow,
                Notes = reason
            };

            _context.Set<DocumentTracking>().Add(tracking);

            // به‌روزرسانی تخصیص‌های مرتبط
            var assignments = await _context.Set<DocumentAssignment>()
                .Where(da => da.DocumentId == documentId && da.Status == "PENDING")
                .ToListAsync();

            foreach (var assignment in assignments)
            {
                assignment.Status = "REJECTED";
                assignment.CompletedDate = DateTime.UtcNow;
                assignment.Notes = reason;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // ایجاد اعلان برای فرستنده
            await CreateNotificationAsync(document.CreatedById, "سند رد شد", $"سند '{document.Subject}' توسط شما رد شد. دلیل: {reason}");

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// ارجاع سند
    /// </summary>
    public async Task<bool> ReferDocumentAsync(int documentId, int fromUserId, List<int> toUserIds, string actionType, string notes = null, DateTime? dueDate = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var document = await _context.Set<Document>()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            foreach (var toUserId in toUserIds)
            {
                // ایجاد ارجاع
                var referral = new DocumentReferral
                {
                    DocumentId = documentId,
                    FromUserId = fromUserId,
                    ToUserId = toUserId,
                    ActionTypeCode = actionType,
                    Notes = notes,
                    DueDate = dueDate,
                    ReferralDate = DateTime.UtcNow,
                    Status = "PENDING"
                };

                _context.Set<DocumentReferral>().Add(referral);

                // ایجاد توزیع
                var distribution = new DocumentDistribution
                {
                    DocumentId = documentId,
                    RecipientUserId = toUserId,
                    DistributionMethod = "REFERRAL",
                    Status = "DISTRIBUTED",
                    DistributedDate = DateTime.UtcNow
                };

                _context.Set<DocumentDistribution>().Add(distribution);

                // ایجاد ردیابی
                var tracking = new DocumentTracking
                {
                    DocumentId = documentId,
                    UserId = toUserId,
                    Status = "REFERRED",
                    ActionPerformed = $"ارجاع با نوع {actionType}",
                    PerformedDate = DateTime.UtcNow,
                    Notes = notes
                };

                _context.Set<DocumentTracking>().Add(tracking);

                // ایجاد اعلان
                await CreateNotificationAsync(toUserId, "سند ارجاع شد", $"سند '{document.Subject}' به شما ارجاع شد.");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// بایگانی سند
    /// </summary>
    public async Task<bool> ArchiveDocumentAsync(int documentId, int archivistId, int archiveFolderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var document = await _context.Set<Document>()
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return false;

            // تغییر وضعیت سند
            document.Status = "ARCHIVED";
            document.ArchivedById = archivistId;
            document.ArchivedDate = DateTime.UtcNow;
            document.ArchiveFolderId = archiveFolderId;
            document.ModifiedDate = DateTime.UtcNow;

            // ایجاد ردیابی
            var tracking = new DocumentTracking
            {
                DocumentId = documentId,
                UserId = archivistId,
                Status = "ARCHIVED",
                ActionPerformed = "بایگانی سند",
                PerformedDate = DateTime.UtcNow
            };

            _context.Set<DocumentTracking>().Add(tracking);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    /// <summary>
    /// ایجاد نسخه جدید از سند
    /// </summary>
    public async Task<Document> CreateNewVersionAsync(int originalDocumentId, int userId, string content, string reason)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var originalDocument = await _context.Set<Document>()
                .FirstOrDefaultAsync(d => d.Id == originalDocumentId);

            if (originalDocument == null)
                return null;

            // ایجاد نسخه جدید
            var newVersion = new Document
            {
                DocumentTypeId = originalDocument.DocumentTypeId,
                DocumentNumber = await GenerateDocumentNumberAsync(originalDocument.DocumentTypeId ?? 0, originalDocument.SecretariatId),
                Subject = originalDocument.Subject,
                Content = content,
                Status = "DRAFT",
                PriorityCode = originalDocument.PriorityCode,
                Classification = originalDocument.Classification,
                CreatedById = userId,
                SecretariatId = originalDocument.SecretariatId,
                CreationDate = DateTime.UtcNow,
                ParentDocumentId = originalDocumentId,
                Version = (originalDocument.Version ?? 1) + 1
            };

            _context.Set<Document>().Add(newVersion);
            await _context.SaveChangesAsync();

            // ایجاد ردیابی
            var tracking = new DocumentTracking
            {
                DocumentId = newVersion.Id,
                UserId = userId,
                Status = "VERSION_CREATED",
                ActionPerformed = "ایجاد نسخه جدید",
                PerformedDate = DateTime.UtcNow,
                Notes = reason
            };

            _context.Set<DocumentTracking>().Add(tracking);

            // به‌روزرسانی سند اصلی
            originalDocument.Status = "SUPERSEDED";
            originalDocument.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return newVersion;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// جستجوی پیشرفته اسناد
    /// </summary>
    public async Task<List<DocumentSearchResult>> SearchDocumentsAsync(DocumentSearchCriteria criteria)
    {
        var query = _context.Set<Document>()
            .Include(d => d.CreatedByUser)
            .AsQueryable();

        // فیلتر بر اساس شماره سند
        if (!string.IsNullOrEmpty(criteria.DocumentNumber))
        {
            query = query.Where(d => d.DocumentNumber.Contains(criteria.DocumentNumber));
        }

        // فیلتر بر اساس موضوع
        if (!string.IsNullOrEmpty(criteria.Subject))
        {
            query = query.Where(d => d.Subject.Contains(criteria.Subject));
        }

        // فیلتر بر اساس محتوا
        if (!string.IsNullOrEmpty(criteria.Content))
        {
            query = query.Where(d => d.Content.Contains(criteria.Content));
        }

        // فیلتر بر اساس وضعیت
        if (criteria.Statuses != null && criteria.Statuses.Any())
        {
            query = query.Where(d => criteria.Statuses.Contains(d.Status));
        }

        // فیلتر بر اساس تاریخ
        if (criteria.FromDate.HasValue)
        {
            query = query.Where(d => d.CreationDate >= criteria.FromDate.Value);
        }

               if (criteria.ToDate.HasValue)
        {
            query = query.Where(d => d.CreationDate <= criteria.ToDate.Value);
        }

        // فیلتر بر اساس اولویت
        if (!string.IsNullOrEmpty(criteria.Priority))
        {
            query = query.Where(d => d.PriorityCode == criteria.Priority);
        }

        // فیلتر بر اساس طبقه‌بندی
        if (!string.IsNullOrEmpty(criteria.Classification))
        {
            query = query.Where(d => d.Classification == criteria.Classification);
        }

        // فیلتر بر اساس نوع سند
        if (criteria.DocumentTypeId.HasValue)
        {
            query = query.Where(d => d.DocumentTypeId == criteria.DocumentTypeId.Value);
        }

        // فیلتر بر اساس کاربر سازنده
        if (criteria.CreatedById.HasValue)
        {
            query = query.Where(d => d.CreatedById == criteria.CreatedById.Value);
        }

        // مرتب‌سازی
        switch (criteria.SortBy)
        {
            case "DateDesc":
                query = query.OrderByDescending(d => d.CreationDate);
                break;
            case "DateAsc":
                query = query.OrderBy(d => d.CreationDate);
                break;
            case "Subject":
                query = query.OrderBy(d => d.Subject);
                break;
            default:
                query = query.OrderByDescending(d => d.CreationDate);
                break;
        }

        // صفحه‌بندی
        var documents = await query
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return documents.Select(d => new DocumentSearchResult
        {
            Id = d.Id,
            DocumentNumber = d.DocumentNumber,
            Subject = d.Subject,
            DocumentTypeName = d.DocumentType,
            CreatedByName = d.CreatedByUser != null ? d.CreatedByUser.FirstName + " " + d.CreatedByUser.LastName : "",
            CreationDate = d.CreationDate,
            Status = d.Status,
            Priority = d.PriorityCode ?? ""
        }).ToList();
    }

    /// <summary>
    /// تولید شماره سند
    /// </summary>
    private async Task<string> GenerateDocumentNumberAsync(int documentTypeId, int? secretariatId)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var typePart = documentTypeId.ToString("D3");
        var sequencePart = await GetNextSequenceAsync($"DOC_{datePart}_{typePart}");

        return $"DOC-{datePart}-{typePart}-{sequencePart:D4}";
    }

    /// <summary>
    /// دریافت شمارنده بعدی
    /// </summary>
    private async Task<int> GetNextSequenceAsync(string sequenceKey)
    {
        var sequence = await _context.Set<SequenceNumber>()
            .FirstOrDefaultAsync(s => s.SequenceKey == sequenceKey);

        if (sequence == null)
        {
            sequence = new SequenceNumber
            {
                SequenceKey = sequenceKey,
                CurrentValue = 1
            };
            _context.Set<SequenceNumber>().Add(sequence);
        }
        else
        {
            sequence.CurrentValue++;
        }

        await _context.SaveChangesAsync();
        return sequence.CurrentValue;
    }

    /// <summary>
    /// ایجاد اعلان
    /// </summary>
    private async Task CreateNotificationAsync(int userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = "DOCUMENT",
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };

        _context.Set<Notification>().Add(notification);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ثبت لاگ سند
    /// </summary>
    private async Task LogDocumentActivity(int documentId, int userId, string action, string details)
    {
        var logEntry = new DocumentLog
        {
            DocumentId = documentId,
            UserId = userId,
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _context.Set<DocumentLog>().Add(logEntry);
        await _context.SaveChangesAsync();
    }
}

/// <summary>
/// درخواست ایجاد سند
/// </summary>
public class DocumentCreationRequest
{
    public int DocumentTypeId { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public int CreatedById { get; set; }
    public int? SecretariatId { get; set; }
    public string Priority { get; set; } = "NORMAL";
    public string Classification { get; set; } = "CONFIDENTIAL";
    public DateTime? DueDate { get; set; }
    public List<DocumentRecipient> Recipients { get; set; } = new List<DocumentRecipient>();
    public List<DocumentAttachmentInfo> Attachments { get; set; } = new List<DocumentAttachmentInfo>();
}

/// <summary>
/// اطلاعات گیرنده سند
/// </summary>
public class DocumentRecipient
{
    public int? UserId { get; set; }
    public int? DepartmentId { get; set; }
    public string DistributionMethod { get; set; } = "EMAIL";
}

/// <summary>
/// اطلاعات پیوست سند
/// </summary>
public class DocumentAttachmentInfo
{
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public string StoragePath { get; set; }
    public string Description { get; set; }
}

/// <summary>
/// معیارهای جستجوی سند
/// </summary>
public class DocumentSearchCriteria
{
    public string DocumentNumber { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public List<string> Statuses { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string Priority { get; set; }
    public string Classification { get; set; }
    public int? DocumentTypeId { get; set; }
    public int? CreatedById { get; set; }
    public string SortBy { get; set; } = "DateDesc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// نتیجه جستجوی سند
/// </summary>
public class DocumentSearchResult
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; }
    public string Subject { get; set; }
    public string DocumentTypeName { get; set; }
    public string CreatedByName { get; set; }
    public DateTime CreationDate { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
}