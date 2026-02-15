using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// قوانین شماره‌گذاری مدارک
/// Document numbering rules
/// </summary>
public class NumberingRule : BaseEntity
{
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
    /// نوع سند مرتبط
    /// </summary>
    public int? DocumentTypeId { get; set; }
    public virtual DocumentType? DocumentType { get; set; }

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
}

/// <summary>
/// حوزه قانون شماره‌گذاری
/// </summary>
public enum NumberingRuleScope
{
    /// <summary>
    /// عمومی - برای تمام سازمان‌ها
    /// </summary>
    General = 1,

    /// <summary>
    /// سازمانی - برای یک سازمان خاص
    /// </summary>
    Organization = 2,

    /// <summary>
    /// دپارتمانی - برای یک دپارتمان خاص
    /// </summary>
    Department = 3,

    /// <summary>
    /// دبیرخانه‌ای - برای یک دبیرخانه خاص
    /// </summary>
    Secretariat = 4,

    /// <summary>
    /// نوع سند - برای یک نوع سند خاص
    /// </summary>
    DocumentType = 5
}

/// <summary>
/// متغیرهای قابل استفاده در الگوی شماره‌گذاری
/// </summary>
public static class NumberingPatternVariables
{
    /// <summary>
    /// سال شمسی 4 رقمی
    /// </summary>
    public const string PersianYear4Digit = "{PY}";

    /// <summary>
    /// سال شمسی 2 رقمی
    /// </summary>
    public const string PersianYear2Digit = "{PY2}";

    /// <summary>
    /// ماه شمسی دو رقمی
    /// </summary>
    public const string PersianMonth2Digit = "{PM}";

    /// <summary>
    /// روز شمسی دو رقمی
    /// </summary>
    public const string PersianDay2Digit = "{PD}";

    /// <summary>
    /// سال میلادی 4 رقمی
    /// </summary>
    public const string GregorianYear4Digit = "{GY}";

    /// <summary>
    /// سال میلادی 2 رقمی
    /// </summary>
    public const string GregorianYear2Digit = "{GY2}";

    /// <summary>
    /// ماه میلادی دو رقمی
    /// </summary>
    public const string GregorianMonth2Digit = "{GM}";

    /// <summary>
    /// روز میلادی دو رقمی
    /// </summary>
    public const string GregorianDay2Digit = "{GD}";

    /// <summary>
    /// سال هجری 4 رقمی
    /// </summary>
    public const string HijriYear4Digit = "{HY}";

    /// <summary>
    /// سال هجری 2 رقمی
    /// </summary>
    public const string HijriYear2Digit = "{HY2}";

    /// <summary>
    /// ماه هجری دو رقمی
    /// </summary>
    public const string HijriMonth2Digit = "{HM}";

    /// <summary>
    /// روز هجری دو رقمی
    /// </summary>
    public const string HijriDay2Digit = "{HD}";

    /// <summary>
    /// شمارنده عددی
    /// </summary>
    public const string Counter = "{C}";

    /// <summary>
    /// شناسه سازمان
    /// </summary>
    public const string OrganizationId = "{ORG_ID}";

    /// <summary>
    /// کد سازمان
    /// </summary>
    public const string OrganizationCode = "{ORG_CODE}";

    /// <summary>
    /// شناسه دپارتمان
    /// </summary>
    public const string DepartmentId = "{DEPT_ID}";

    /// <summary>
    /// کد دپارتمان
    /// </summary>
    public const string DepartmentCode = "{DEPT_CODE}";

    /// <summary>
    /// شناسه دبیرخانه
    /// </summary>
    public const string SecretariatId = "{SEC_ID}";

    /// <summary>
    /// کد دبیرخانه
    /// </summary>
    public const string SecretariatCode = "{SEC_CODE}";

    /// <summary>
    /// شناسه نوع سند
    /// </summary>
    public const string DocumentTypeId = "{DOC_TYPE_ID}";

    /// <summary>
    /// کد نوع سند
    /// </summary>
    public const string DocumentTypeCode = "{DOC_TYPE_CODE}";

    /// <summary>
    /// حرف تصادفی
    /// </summary>
    public const string RandomChar = "{RND}";
}