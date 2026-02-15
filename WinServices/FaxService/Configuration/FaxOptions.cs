namespace Automation.FaxService.Configuration;

/// <summary>
/// تنظیمات سرویس فکس برای اتوماسیون
/// </summary>
public class FaxOptions
{
    public const string SectionName = "Fax";

    /// <summary>فعال بودن ارسال فکس (Twilio)</summary>
    public bool SendEnabled { get; set; } = true;

    /// <summary>شناسه حساب Twilio</summary>
    public string? TwilioAccountSid { get; set; }

    /// <summary>توکن احراز هویت Twilio</summary>
    public string? TwilioAuthToken { get; set; }

    /// <summary>شماره فرستنده فکس (باید قابلیت Fax داشته باشد)</summary>
    public string? TwilioFromNumber { get; set; }

    /// <summary>پوشه صف ارسال - اتوماسیون درخواست‌ها را اینجا قرار می‌دهد</summary>
    public string OutboxPath { get; set; } = "FaxOutbox";

    /// <summary>پوشه ذخیره فکس‌های دریافتی</summary>
    public string InboxPath { get; set; } = "FaxInbox";

    /// <summary>فاصله بررسی Outbox (ثانیه)</summary>
    public int PollIntervalSeconds { get; set; } = 45;

    /// <summary>آدرس پایه برای دسترسی به فایل‌های PDF توسط Twilio (مثلاً https://your-server.com/faxfiles/)</summary>
    public string? BaseUrlForMedia { get; set; }

    /// <summary>آدرس Webhook برای دریافت فکس (برای تنظیم در پنل Twilio)</summary>
    public string? IncomingWebhookPath { get; set; } = "/fax/incoming";
}
