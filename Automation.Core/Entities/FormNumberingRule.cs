using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// قوانین شماره‌گذاری برای فرم‌ها
/// Form numbering rules
/// </summary>
public class FormNumberingRule : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;

    /// <summary>
    /// کد قانون شماره‌گذاری
    /// </summary>
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>
    /// نام قانون شماره‌گذاری
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات قانون
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// الگوی شماره‌گذاری
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// پیشوند شماره
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// پسوند شماره
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// طول شمارنده عددی
    /// </summary>
    public int CounterLength { get; set; } = 4;

    /// <summary>
    /// مقدار شروع شمارنده
    /// </summary>
    public int StartCounter { get; set; } = 1;

    /// <summary>
    /// آخرین مقدار استفاده شده شمارنده
    /// </summary>
    public int LastCounter { get; set; } = 0;

    /// <summary>
    /// آیا شامل تاریخ است؟
    /// </summary>
    public bool IncludeDate { get; set; } = true;

    /// <summary>
    /// فرمت تاریخ (YYYY, YY, MM, DD و...)
    /// </summary>
    public string DateFormat { get; set; } = "yyyy";

    /// <summary>
    /// جداکننده بین بخش‌ها
    /// </summary>
    public string Separator { get; set; } = "-";

    /// <summary>
    /// آیا قانون فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// دبیرخانه مرتبط
    /// </summary>
    public int? SecretariatId { get; set; }
    public virtual Secretariat? Secretariat { get; set; }

    /// <summary>
    /// دپارتمان مرتبط
    /// </summary>
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    /// <summary>
    /// سازمان مرتبط
    /// </summary>
    public int? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    /// <summary>
    /// نوع قانون (عمومی، سازمانی، دبیرخانه‌ای)
    /// </summary>
    public NumberingRuleScope Scope { get; set; } = NumberingRuleScope.General;

    /// <summary>
    /// آیا قابل تنظیم توسط کاربر است؟
    /// </summary>
    public bool IsUserConfigurable { get; set; } = false;

    /// <summary>
    /// تنظیمات پیشرفته (JSON)
    /// </summary>
    public string AdvancedSettings { get; set; } = "{}";

    /// <summary>
    /// آیا این قانون پیش‌فرض است؟
    /// </summary>
    public bool IsDefault { get; set; } = false;
}