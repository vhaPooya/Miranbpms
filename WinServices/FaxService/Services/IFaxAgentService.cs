namespace Automation.FaxService.Services;

/// <summary>
/// سرویس عامل فکس: ارسال و دریافت برای اتوماسیون
/// </summary>
public interface IFaxAgentService
{
    /// <summary>ارسال فکس</summary>
    Task SendAsync(Models.OutboxFaxRequest request, CancellationToken cancellationToken = default);

    /// <summary>پردازش تمام درخواست‌های Outbox</summary>
    Task ProcessOutboxAsync(CancellationToken cancellationToken = default);

    /// <summary>ذخیره فکس دریافتی (از Webhook Twilio)</summary>
    Task SaveIncomingFaxAsync(Stream pdfStream, string fromNumber, string sid, CancellationToken cancellationToken = default);
}
