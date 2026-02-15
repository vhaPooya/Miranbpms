using Automation.Core.DTOs;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت فکس
/// </summary>
public interface IFaxService
{
    /// <summary>
    /// ارسال فکس
    /// </summary>
    Task<FaxSendResult> SendFaxAsync(int settingsId, string toFaxNumber, string filePath, string? fromFaxNumber = null, string? fromName = null, string? toName = null, int? documentId = null, int? senderUserId = null);
    
    /// <summary>
    /// دریافت فکس‌ها
    /// </summary>
    Task<List<FaxMessageDto>> ReceiveFaxesAsync(int settingsId, int maxCount = 50);
    
    /// <summary>
    /// دریافت لیست فکس‌ها
    /// </summary>
    Task<List<FaxMessageDto>> GetFaxMessagesAsync(int? settingsId = null, string? messageType = null, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// دریافت فکس بر اساس شناسه
    /// </summary>
    Task<FaxMessageDto?> GetFaxMessageAsync(int faxMessageId);
}




