namespace Automation.EmailService.Models;

/// <summary>
/// مدل درخواست ارسال ایمیل که اتوماسیون در پوشه Outbox قرار می‌دهد
/// نام فایل: هر نام با پسوند .email.json
/// </summary>
public class OutboxEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    /// <summary>true = Body به صورت HTML است</summary>
    public bool IsBodyHtml { get; set; } = true;
    /// <summary>مسیرهای نسبی یا مطلق فایل‌های پیوست (نسبت به پوشه Outbox یا مطلق)</summary>
    public List<string>? AttachmentPaths { get; set; }
    /// <summary>شناسه مرجع از طرف اتوماسیون برای ردیابی</summary>
    public string? ReferenceId { get; set; }
}
