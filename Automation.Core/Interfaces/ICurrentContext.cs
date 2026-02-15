namespace Automation.Core.Interfaces;

/// <summary>
/// زمینه جاری درخواست — در سراسر پروژه با نام‌های ثابت (OUserId, OPosId, OFEIC, OFEC و ...) در دسترس است.
/// </summary>
public interface ICurrentContext
{
    /// <summary>
    /// شناسه کاربر جاری (کاربر لاگین‌کرده) — OUserId
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// شناسه سمت/پوزیشن جاری — OPosId
    /// </summary>
    int? PositionId { get; }

    /// <summary>
    /// شناسه یا کد فرم جاری (فرم در حال اجرا / فرم متصل به فرآیند) — OFEIC
    /// </summary>
    int? FormId { get; }

    /// <summary>
    /// کد فرم جاری (FormCode) در صورت نیاز
    /// </summary>
    string? FormCode { get; }

    /// <summary>
    /// شناسه رکورد در حال ثبت یا ویرایش — OFEC
    /// </summary>
    long? RecordId { get; }

    /// <summary>
    /// شناسه سازمان جاری — OOrganizationId
    /// </summary>
    int? OrganizationId { get; }

    /// <summary>
    /// شناسه دپارتمان جاری — ODepartmentId
    /// </summary>
    int? DepartmentId { get; }

    /// <summary>
    /// شناسه دبیرخانه جاری — OSecretariatId
    /// </summary>
    int? SecretariatId { get; }

    /// <summary>
    /// شناسه نمونه فرآیند جاری — OWorkflowInstanceId
    /// </summary>
    int? WorkflowInstanceId { get; }

    /// <summary>
    /// مقدار یک آیتم با نام استاندارد (مثلاً CurrentContextNames.OUserId)
    /// </summary>
    object? GetValue(string contextKey);
}
