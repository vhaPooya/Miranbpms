namespace Automation.EmailService.Configuration;

/// <summary>
/// تنظیمات سرویس ایمیل برای اتوماسیون
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>تنظیمات SMTP برای ارسال</summary>
    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>تنظیمات IMAP برای دریافت</summary>
    public ImapOptions Imap { get; set; } = new();

    /// <summary>پوشه صف ارسال - پروژه اتوماسیون فایل‌های درخواست ارسال را اینجا قرار می‌دهد</summary>
    public string OutboxPath { get; set; } = "EmailOutbox";

    /// <summary>پوشه ذخیره ایمیل‌های دریافتی برای مصرف اتوماسیون</summary>
    public string InboxPath { get; set; } = "EmailInbox";

    /// <summary>فاصله زمانی بررسی صندوق ورودی (ثانیه)</summary>
    public int PollIntervalSeconds { get; set; } = 60;

    /// <summary>فعال بودن دریافت ایمیل</summary>
    public bool ReceiveEnabled { get; set; } = true;

    /// <summary>فعال بودن ارسال ایمیل</summary>
    public bool SendEnabled { get; set; } = true;
}

public class SmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? DefaultFromAddress { get; set; }
    public string? DefaultFromName { get; set; }
}

public class ImapOptions
{
    public string Host { get; set; } = "imap.gmail.com";
    public int Port { get; set; } = 993;
    public bool UseSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string InboxFolderName { get; set; } = "INBOX";
}
