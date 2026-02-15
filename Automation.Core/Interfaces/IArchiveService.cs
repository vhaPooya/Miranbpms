using Automation.Core.DTOs;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت دبیرخانه
/// </summary>
public interface IArchiveService
{
    /// <summary>
    /// ثبت نامه وارده
    /// </summary>
    Task<RegisterIncomingResult> RegisterIncomingDocumentAsync(RegisterIncomingDto dto);
    
    /// <summary>
    /// ثبت نامه صادره
    /// </summary>
    Task<RegisterOutgoingResult> RegisterOutgoingDocumentAsync(RegisterOutgoingDto dto);
    
    /// <summary>
    /// دریافت شماره وارده بعدی
    /// </summary>
    Task<string> GetNextIncomingNumberAsync(int year, int? organizationId = null, int? departmentId = null, int? secretariatId = null, int? documentTypeId = null);
    
    /// <summary>
    /// دریافت شماره صادره بعدی
    /// </summary>
    Task<string> GetNextOutgoingNumberAsync(int year, int? organizationId = null, int? departmentId = null, int? secretariatId = null, int? documentTypeId = null);
    
    /// <summary>
    /// بایگانی کردن مدرک
    /// </summary>
    Task<bool> ArchiveDocumentAsync(int documentId, int userId, string? archiveLocation = null);
    
    /// <summary>
    /// دریافت لیست مدارک بایگانی شده
    /// </summary>
    Task<ArchiveListDto> GetArchivedDocumentsAsync(int page = 1, int pageSize = 10, string? search = null, DateTime? fromDate = null, DateTime? toDate = null);
}




