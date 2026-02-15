using Automation.Core.Entities;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت مدارک
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// پیوست کردن فایل خارجی
    /// </summary>
    Task<bool> AttachExternalFileAsync(int documentId, int formId, string fileName, string filePath, long fileSize, string mimeType, int userId, string? description = null);
    
    /// <summary>
    /// پیوست کردن مدرک داخلی
    /// </summary>
    Task<bool> AttachInternalDocumentAsync(int documentId, int formId, int referencedDocumentId, int referencedFormId, int userId, string category = "ATTACHMENT", string? description = null);
    
    /// <summary>
    /// عطف کردن مدرک
    /// </summary>
    Task<bool> ReferenceDocumentAsync(int documentId, int formId, int referencedDocumentId, int referencedFormId, int userId, string? description = null);
    
    /// <summary>
    /// دریافت لیست پیوست‌های مدرک
    /// </summary>
    Task<List<DocumentAttachment>> GetDocumentAttachmentsAsync(int documentId);
    
    /// <summary>
    /// دریافت لیست عطف‌های مدرک
    /// </summary>
    Task<List<DocumentAttachment>> GetDocumentReferencesAsync(int documentId);
    
    /// <summary>
    /// حذف پیوست
    /// </summary>
    Task<bool> DeleteAttachmentAsync(int attachmentId, int userId);
    
    /// <summary>
    /// امضاء کردن مدرک
    /// </summary>
    Task<bool> SignDocumentAsync(int documentId, int userId);
    
    /// <summary>
    /// بررسی آیا مدرک امضاء شده است؟
    /// </summary>
    Task<bool> IsDocumentSignedAsync(int documentId);
    
    /// <summary>
    /// ارجاع مدرک
    /// </summary>
    Task<bool> ReferDocumentAsync(int documentId, int formId, int actionTypeId, int referredByUserId, int? referredToUserId = null, int? referredToRoleId = null, int? referredToDepartmentId = null, int? referredToGroupId = null, string? notes = null, DateTime? dueDate = null);
}




