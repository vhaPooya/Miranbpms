using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت مدارک
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly AutomationDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(AutomationDbContext context, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AttachExternalFileAsync(int documentId, int formId, string fileName, string filePath, long fileSize, string mimeType, int userId, string? description = null)
    {
        try
        {
            var attachment = new DocumentAttachment
            {
                DocumentId = documentId,
                FormId = formId,
                AttachmentType = "EXTERNAL_FILE",
                AttachmentCategory = "ATTACHMENT",
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileSize,
                MimeType = mimeType,
                Description = description,
                UploadedByUserId = userId,
                UploadedDate = DateTime.UtcNow
            };

            _context.DocumentAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("External file attached to document {DocumentId} by user {UserId}", documentId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching external file to document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<bool> AttachInternalDocumentAsync(int documentId, int formId, int referencedDocumentId, int referencedFormId, int userId, string category = "ATTACHMENT", string? description = null)
    {
        try
        {
            var attachment = new DocumentAttachment
            {
                DocumentId = documentId,
                FormId = formId,
                AttachmentType = "INTERNAL_DOCUMENT",
                AttachmentCategory = category,
                ReferencedDocumentId = referencedDocumentId,
                ReferencedFormId = referencedFormId,
                Description = description,
                UploadedByUserId = userId,
                UploadedDate = DateTime.UtcNow
            };

            _context.DocumentAttachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Internal document attached to document {DocumentId} by user {UserId}", documentId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching internal document to document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<bool> ReferenceDocumentAsync(int documentId, int formId, int referencedDocumentId, int referencedFormId, int userId, string? description = null)
    {
        return await AttachInternalDocumentAsync(documentId, formId, referencedDocumentId, referencedFormId, userId, "REFERENCE", description);
    }

    public async Task<List<DocumentAttachment>> GetDocumentAttachmentsAsync(int documentId)
    {
        try
        {
            return await _context.DocumentAttachments
                .Include(da => da.Form)
                .Include(da => da.ReferencedForm)
                .Include(da => da.UploadedByUser)
                .Where(da => da.DocumentId == documentId && 
                            da.AttachmentCategory == "ATTACHMENT" && 
                            !da.IsDeleted)
                .OrderByDescending(da => da.UploadedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attachments for document {DocumentId}", documentId);
            return new List<DocumentAttachment>();
        }
    }

    public async Task<List<DocumentAttachment>> GetDocumentReferencesAsync(int documentId)
    {
        try
        {
            return await _context.DocumentAttachments
                .Include(da => da.Form)
                .Include(da => da.ReferencedForm)
                .Include(da => da.UploadedByUser)
                .Where(da => da.DocumentId == documentId && 
                            da.AttachmentCategory == "REFERENCE" && 
                            !da.IsDeleted)
                .OrderByDescending(da => da.UploadedDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting references for document {DocumentId}", documentId);
            return new List<DocumentAttachment>();
        }
    }

    public async Task<bool> DeleteAttachmentAsync(int attachmentId, int userId)
    {
        try
        {
            var attachment = await _context.DocumentAttachments.FindAsync(attachmentId);
            if (attachment == null)
                return false;

            // TODO: بررسی آیا مدرک امضاء شده است؟ اگر بله، نمی‌توان حذف کرد
            // var isSigned = await IsDocumentSignedAsync(attachment.DocumentId);
            // if (isSigned)
            //     return false;

            attachment.IsDeleted = true;
            attachment.EditDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Attachment {AttachmentId} deleted by user {UserId}", attachmentId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting attachment {AttachmentId}", attachmentId);
            return false;
        }
    }

    public async Task<bool> SignDocumentAsync(int documentId, int userId)
    {
        try
        {
            // TODO: پیاده‌سازی امضاء مدرک
            // باید در جدول FormData یا Document فیلد IsSigned را true کنیم
            // var document = await _context.Documents.FindAsync(documentId);
            // if (document == null)
            //     return false;
            
            // document.IsSigned = true;
            // document.SignedByUserId = userId;
            // document.SignedDate = DateTime.UtcNow;
            // await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} signed by user {UserId}", documentId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<bool> IsDocumentSignedAsync(int documentId)
    {
        try
        {
            // TODO: بررسی از جدول FormData یا Document
            // var document = await _context.Documents.FindAsync(documentId);
            // return document?.IsSigned == true;
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking document sign status for document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<bool> ReferDocumentAsync(int documentId, int formId, int actionTypeId, int referredByUserId, int? referredToUserId = null, int? referredToRoleId = null, int? referredToDepartmentId = null, int? referredToGroupId = null, string? notes = null, DateTime? dueDate = null)
    {
        try
        {
            var referral = new DocumentReferral
            {
                DocumentId = documentId,
                FormId = formId,
                ActionTypeId = actionTypeId,
                ReferredByUserId = referredByUserId,
                ReferredToUserId = referredToUserId,
                ReferredToRoleId = referredToRoleId,
                ReferredToDepartmentId = referredToDepartmentId,
                ReferredToGroupId = referredToGroupId,
                Notes = notes,
                DueDate = dueDate,
                Status = "PENDING",
                ReferredDate = DateTime.UtcNow
            };

            _context.DocumentReferrals.Add(referral);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} referred by user {UserId}", documentId, referredByUserId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error referring document {DocumentId}", documentId);
            return false;
        }
    }
}




