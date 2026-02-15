using Automation.EmailService.Models;

namespace Automation.EmailService.Services;

/// <summary>
/// سرویس عامل ایمیل: ارسال و دریافت برای اتوماسیون
/// </summary>
public interface IEmailAgentService
{
    /// <summary>ارسال یک ایمیل</summary>
    Task SendAsync(OutboxEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>بررسی و ارسال تمام درخواست‌های موجود در پوشه Outbox</summary>
    Task ProcessOutboxAsync(CancellationToken cancellationToken = default);

    /// <summary>بررسی صندوق IMAP و ذخیره ایمیل‌های جدید در Inbox</summary>
    Task ReceiveAndStoreAsync(CancellationToken cancellationToken = default);
}
