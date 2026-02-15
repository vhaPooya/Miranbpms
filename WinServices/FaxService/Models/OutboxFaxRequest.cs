namespace Automation.FaxService.Models;

/// <summary>
/// درخواست ارسال فکس - فایل JSON در پوشه Outbox با نام *.fax.json
/// همراه با فایل PDF با همین نام (بدون پسوند .fax.json)
/// یا استفاده از MediaUrl در صورت در دسترس بودن URL عمومی
/// </summary>
public class OutboxFaxRequest
{
    /// <summary>شماره مقصد فکس (با کد کشور، مثلاً +982112345678)</summary>
    public string To { get; set; } = string.Empty;

    /// <summary>آدرس عمومی فایل PDF (در صورت نبود، از فایل هم‌نام در Outbox با BaseUrlForMedia استفاده می‌شود)</summary>
    public string? MediaUrl { get; set; }

    /// <summary>نام فایل PDF در پوشه Outbox (مثلاً document.pdf) - در این صورت BaseUrlForMedia ضروری است</summary>
    public string? FileName { get; set; }

    /// <summary>شناسه مرجع از طرف اتوماسیون</summary>
    public string? ReferenceId { get; set; }
}
