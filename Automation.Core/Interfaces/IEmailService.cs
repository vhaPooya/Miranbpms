using Automation.Core.DTOs;

namespace Automation.Core.Interfaces;

/// <summary>
/// سرویس مدیریت ایمیل
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// ارسال ایمیل
    /// </summary>
    Task<EmailSendResult> SendEmailAsync(int settingsId, string toEmail, string subject, string body, bool isHtml = false, string? fromEmail = null, string? fromName = null, string? cc = null, string? bcc = null, List<string>? attachments = null, int? documentId = null, int? senderUserId = null);
    
    /// <summary>
    /// دریافت ایمیل‌ها
    /// </summary>
    Task<List<EmailMessageDto>> ReceiveEmailsAsync(int settingsId, int maxCount = 50);
    
    /// <summary>
    /// دریافت لیست ایمیل‌ها
    /// </summary>
    Task<List<EmailMessageDto>> GetEmailMessagesAsync(int? settingsId = null, string? messageType = null, string? status = null, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// دریافت ایمیل بر اساس شناسه
    /// </summary>
    Task<EmailMessageDto?> GetEmailMessageAsync(int emailMessageId);
}




