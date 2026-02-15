namespace Automation.Core.Interfaces;

/// <summary>
/// تنظیم زمینه جاری فرم/رکورد/فرآیند برای همین درخواست (مثلاً در کنترلر فرم یا فرآیند).
/// </summary>
public interface ICurrentContextSetter
{
    /// <summary>
    /// تنظیم فرم و رکورد جاری برای این درخواست — OFEIC, OFEC
    /// </summary>
    void SetFormContext(int? formId, long? recordId = null);

    /// <summary>
    /// تنظیم کد فرم جاری (اختیاری، در غیر این صورت از FormId استخراج می‌شود)
    /// </summary>
    void SetFormCode(string? formCode);

    /// <summary>
    /// تنظیم نمونه فرآیند جاری برای این درخواست — OWorkflowInstanceId
    /// </summary>
    void SetWorkflowInstanceId(int? workflowInstanceId);

    /// <summary>
    /// پاک کردن زمینه فرم/رکورد/فرآیند این درخواست
    /// </summary>
    void ClearRequestContext();
}
