using Automation.Core.Entities;
using Automation.Core.DTOs;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت کارتابل‌ها
/// </summary>
public interface ICabinetService
{
    /// <summary>
    /// دریافت کارتابل ورودی کاربر
    /// </summary>
    Task<CabinetListDto> GetInboxAsync(int userId, int page = 1, int pageSize = 10, string? search = null, string? status = null);
    
    /// <summary>
    /// دریافت کارتابل ارجاعی کاربر
    /// </summary>
    Task<CabinetListDto> GetReferredAsync(int userId, int page = 1, int pageSize = 10, string? search = null, string? status = null);
    
    /// <summary>
    /// دریافت کارتابل صادره کاربر
    /// </summary>
    Task<CabinetListDto> GetOutboxAsync(int userId, int page = 1, int pageSize = 10, string? search = null);
    
    /// <summary>
    /// دریافت کارتابل داخلی کاربر
    /// </summary>
    Task<CabinetListDto> GetInternalAsync(int userId, int page = 1, int pageSize = 10, string? search = null);
    
    /// <summary>
    /// دریافت کارتابل شخصی کاربر
    /// </summary>
    Task<CabinetListDto> GetPersonalAsync(int userId, int page = 1, int pageSize = 10, string? search = null);
    
    /// <summary>
    /// ثبت مشاهده مدرک
    /// </summary>
    Task<bool> MarkAsViewedAsync(int documentId, int userId);
    
    /// <summary>
    /// ثبت بررسی مدرک
    /// </summary>
    Task<bool> MarkAsReviewedAsync(int documentId, int userId, bool isReviewed);
    
    /// <summary>
    /// دریافت جزئیات مدرک برای کارتابل
    /// </summary>
    Task<CabinetDocumentDto?> GetDocumentDetailsAsync(int documentId, int userId);
}




