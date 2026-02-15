namespace Automation.Core.Constants;

/// <summary>
/// نام‌های استاندارد آیتم‌های زمینه جاری در سراسر پروژه BPMS.
/// این نام‌ها در View، اسکریپت‌ها، و سرویس‌ها برای دسترسی یکسان استفاده می‌شوند.
/// </summary>
public static class CurrentContextNames
{
    /// <summary>
    /// کاربر جاری (همان کاربر لاگین‌کرده) — شناسه کاربر
    /// </summary>
    public const string OUserId = "OUserId";

    /// <summary>
    /// سمت یا پوزیشن جاری — شناسه پوزیشن
    /// </summary>
    public const string OPosId = "OPosId";

    /// <summary>
    /// کد/شناسه فرم جاری (فرمی که برای ثبت اطلاعات اجرا شده؛ در فرآیندساز، فرم متصل به فرآیند)
    /// </summary>
    public const string OFEIC = "OFEIC";

    /// <summary>
    /// شناسه رکوردی که در حال وارد کردن/ثبت آن هستیم (رکوردی که تولید خواهد شد یا در حال ویرایش است)
    /// </summary>
    public const string OFEC = "OFEC";

    /// <summary>
    /// سازمان جاری کاربر
    /// </summary>
    public const string OOrganizationId = "OOrganizationId";

    /// <summary>
    /// دپارتمان/واحد جاری کاربر
    /// </summary>
    public const string ODepartmentId = "ODepartmentId";

    /// <summary>
    /// دبیرخانه جاری
    /// </summary>
    public const string OSecretariatId = "OSecretariatId";

    /// <summary>
    /// نمونه فرآیند جاری (Workflow Instance) در صورت اجرا در زمینه یک فرآیند
    /// </summary>
    public const string OWorkflowInstanceId = "OWorkflowInstanceId";
}
