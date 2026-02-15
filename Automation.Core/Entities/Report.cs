using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// گزارش‌های ساخته شده در گزارش‌ساز
/// Reports designed in Report Builder
/// </summary>
public class Report : BaseEntity
{
    /// <summary>
    /// کد شناسه گزارش (کلید بیزینس - 4 رقمی)
    /// </summary>
    [Range(1000, 9999, ErrorMessage = "کد گزارش باید 4 رقمی باشد")]
    public int Code { get; set; }

    /// <summary>
    /// کد نمایشی گزارش (مثل RPT-001)
    /// </summary>
    public string ReportCode { get; set; } = string.Empty;

    /// <summary>
    /// نام فارسی گزارش
    /// </summary>
    public string NameFa { get; set; } = string.Empty;

    /// <summary>
    /// نام انگلیسی گزارش
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات گزارش
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// دسته‌بندی گزارش
    /// </summary>
    public int? CategoryId { get; set; }
    public virtual ReportCategory? Category { get; set; }

    /// <summary>
    /// نوع گزارش
    /// </summary>
    public ReportType ReportType { get; set; } = ReportType.Tabular;

    /// <summary>
    /// منبع داده گزارش
    /// </summary>
    public ReportDataSource DataSource { get; set; } = ReportDataSource.Database;

    /// <summary>
    /// فرم مرتبط با گزارش
    /// </summary>
    public int? FormId { get; set; }
    public virtual Form? Form { get; set; }

    /// <summary>
    /// کوئری SQL گزارش
    /// </summary>
    public string? SqlQuery { get; set; }

    /// <summary>
    /// داده‌های طراحی گزارش (JSON کامل)
    /// </summary>
    public string? DesignData { get; set; }

    /// <summary>
    /// تنظیمات فیلترها (JSON)
    /// </summary>
    public string FilterSettings { get; set; } = "{}";

    /// <summary>
    /// تنظیمات گروه‌بندی (JSON)
    /// </summary>
    public string GroupSettings { get; set; } = "{}";

    /// <summary>
    /// تنظیمات مرتب‌سازی (JSON)
    /// </summary>
    public string SortSettings { get; set; } = "{}";

    /// <summary>
    /// تنظیمات قالب‌بندی (JSON)
    /// </summary>
    public string FormatSettings { get; set; } = "{}";

    /// <summary>
    /// آیا گزارش عمومی است؟
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// آیا گزارش فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// آیا گزارش منتشر شده؟
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// تاریخ انتشار
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// تعداد دفعات اجرا
    /// </summary>
    public int ExecutionCount { get; set; } = 0;

    /// <summary>
    /// آخرین تاریخ اجرا
    /// </summary>
    public DateTime? LastExecutedAt { get; set; }

    /// <summary>
    /// فیلدهای گزارش
    /// </summary>
    public virtual ICollection<ReportField> Fields { get; set; } = new List<ReportField>();

    /// <summary>
    /// فیلترهای گزارش
    /// </summary>
    public virtual ICollection<ReportFilter> Filters { get; set; } = new List<ReportFilter>();

    /// <summary>
    /// گروه‌های گزارش
    /// </summary>
    public virtual ICollection<ReportGroup> Groups { get; set; } = new List<ReportGroup>();

    /// <summary>
    /// مرتب‌سازی‌های گزارش
    /// </summary>
    public virtual ICollection<ReportSort> Sorts { get; set; } = new List<ReportSort>();

    /// <summary>
    /// خروجی‌های گزارش
    /// </summary>
    public virtual ICollection<ReportOutput> Outputs { get; set; } = new List<ReportOutput>();
}

/// <summary>
/// دسته‌بندی گزارش‌ها
/// </summary>
public class ReportCategory : BaseEntity
{
    /// <summary>
    /// کد دسته‌بندی
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// نام فارسی دسته‌بندی
    /// </summary>
    public string NameFa { get; set; } = string.Empty;

    /// <summary>
    /// نام انگلیسی دسته‌بندی
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// آیکون
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// دسته‌بندی والد
    /// </summary>
    public int? ParentCategoryId { get; set; }
    public virtual ReportCategory? ParentCategory { get; set; }

    /// <summary>
    /// زیرمجموعه‌ها
    /// </summary>
    public virtual ICollection<ReportCategory> SubCategories { get; set; } = new List<ReportCategory>();

    /// <summary>
    /// گزارش‌های این دسته‌بندی
    /// </summary>
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}

/// <summary>
/// فیلد گزارش
/// </summary>
public class ReportField : BaseEntity
{
    /// <summary>
    /// شناسه گزارش
    /// </summary>
    public int ReportId { get; set; }
    public virtual Report Report { get; set; } = null!;

    /// <summary>
    /// نام فیلد
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// عنوان فارسی
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// نوع داده فیلد
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// منبع فیلد (جدول.ستون)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// آیا قابل نمایش است؟
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// آیا قابل مرتب‌سازی است؟
    /// </summary>
    public bool IsSortable { get; set; } = true;

    /// <summary>
    /// آیا قابل فیلتر است؟
    /// </summary>
    public bool IsFilterable { get; set; } = true;

    /// <summary>
    /// تنظیمات قالب‌بندی (JSON)
    /// </summary>
    public string FormatSettings { get; set; } = "{}";

    /// <summary>
    /// عرض ستون
    /// </summary>
    public int? ColumnWidth { get; set; }

    /// <summary>
    /// تراز متن
    /// </summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;
}

/// <summary>
/// فیلتر گزارش
/// </summary>
public class ReportFilter : BaseEntity
{
    /// <summary>
    /// شناسه گزارش
    /// </summary>
    public int ReportId { get; set; }
    public virtual Report Report { get; set; } = null!;

    /// <summary>
    /// نام فیلد
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// نوع فیلتر
    /// </summary>
    public FilterType FilterType { get; set; }

    /// <summary>
    /// عنوان فارسی
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// مقدار پیش‌فرض
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// آیا فیلتر الزامی است؟
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// تنظیمات فیلتر (JSON)
    /// </summary>
    public string Settings { get; set; } = "{}";
}

/// <summary>
/// گروه‌بندی گزارش
/// </summary>
public class ReportGroup : BaseEntity
{
    /// <summary>
    /// شناسه گزارش
    /// </summary>
    public int ReportId { get; set; }
    public virtual Report Report { get; set; } = null!;

    /// <summary>
    /// نام فیلد
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// عنوان فارسی
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// ترتیب گروه‌بندی
    /// </summary>
    public int GroupOrder { get; set; }

    /// <summary>
    /// آیا قابل گستردن/جمع کردن است؟
    /// </summary>
    public bool IsExpandable { get; set; } = true;
}

/// <summary>
/// مرتب‌سازی گزارش
/// </summary>
public class ReportSort : BaseEntity
{
    /// <summary>
    /// شناسه گزارش
    /// </summary>
    public int ReportId { get; set; }
    public virtual Report Report { get; set; } = null!;

    /// <summary>
    /// نام فیلد
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// جهت مرتب‌سازی
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// ترتیب مرتب‌سازی
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// خروجی گزارش
/// </summary>
public class ReportOutput : BaseEntity
{
    /// <summary>
    /// شناسه گزارش
    /// </summary>
    public int ReportId { get; set; }
    public virtual Report Report { get; set; } = null!;

    /// <summary>
    /// نوع خروجی
    /// </summary>
    public OutputType OutputType { get; set; }

    /// <summary>
    /// نام فایل خروجی
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// مسیر فایل خروجی
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// اندازه فایل
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// تاریخ تولید
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// شناسه کاربر ایجادکننده
    /// </summary>
    public int? GeneratedByUserId { get; set; }

    /// <summary>
    /// پارامترهای استفاده شده (JSON)
    /// </summary>
    public string Parameters { get; set; } = "{}";

    /// <summary>
    /// تعداد رکوردهای خروجی
    /// </summary>
    public int RecordCount { get; set; } = 0;
}

/// <summary>
/// انواع گزارش
/// </summary>
public enum ReportType
{
    /// <summary>
    /// جدولی
    /// </summary>
    Tabular = 1,

    /// <summary>
    /// نموداری
    /// </summary>
    Chart = 2,

    /// <summary>
    /// گزارش داشبورد
    /// </summary>
    Dashboard = 3,

    /// <summary>
    /// گزارش خلاصه
    /// </summary>
    Summary = 4,

    /// <summary>
    /// گزارش مقایسه‌ای
    /// </summary>
    Comparative = 5
}

/// <summary>
/// منابع داده گزارش
/// </summary>
public enum ReportDataSource
{
    /// <summary>
    /// پایگاه داده
    /// </summary>
    Database = 1,

    /// <summary>
    /// وب سرویس
    /// </summary>
    WebService = 2,

    /// <summary>
    /// فایل
    /// </summary>
    File = 3,

    /// <summary>
    /// فرم
    /// </summary>
    Form = 4,

    /// <summary>
    /// فرآیند
    /// </summary>
    Workflow = 5
}

/// <summary>
/// انواع فیلتر
/// </summary>
public enum FilterType
{
    /// <summary>
    /// برابری
    /// </summary>
    Equal = 1,

    /// <summary>
    /// نابرابری
    /// </summary>
    NotEqual = 2,

    /// <summary>
    /// بزرگتر
    /// </summary>
    GreaterThan = 3,

    /// <summary>
    /// کوچکتر
    /// </summary>
    LessThan = 4,

    /// <summary>
    /// بین دو مقدار
    /// </summary>
    Between = 5,

    /// <summary>
    /// شامل
    /// </summary>
    Contains = 6,

    /// <summary>
    /// شروع با
    /// </summary>
    StartsWith = 7,

    /// <summary>
    /// پایان با
    /// </summary>
    EndsWith = 8,

    /// <summary>
    /// در لیست
    /// </summary>
    InList = 9,

    /// <summary>
    /// خالی
    /// </summary>
    IsNull = 10,

    /// <summary>
    /// پر
    /// </summary>
    IsNotNull = 11
}

/// <summary>
/// جهت مرتب‌سازی
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// صعودی
    /// </summary>
    Ascending = 1,

    /// <summary>
    /// نزولی
    /// </summary>
    Descending = 2
}

/// <summary>
/// انواع خروجی
/// </summary>
public enum OutputType
{
    /// <summary>
    /// PDF
    /// </summary>
    Pdf = 1,

    /// <summary>
    /// Excel
    /// </summary>
    Excel = 2,

    /// <summary>
    /// Word
    /// </summary>
    Word = 3,

    /// <summary>
    /// CSV
    /// </summary>
    Csv = 4,

    /// <summary>
    /// HTML
    /// </summary>
    Html = 5,

    /// <summary>
    /// تصویر
    /// </summary>
    Image = 6
}

/// <summary>
/// تراز متن
/// </summary>
public enum TextAlignment
{
    /// <summary>
    /// چپ
    /// </summary>
    Left = 1,

    /// <summary>
    /// راست
    /// </summary>
    Right = 2,

    /// <summary>
    /// وسط
    /// </summary>
    Center = 3,

    /// <summary>
    ///.justify
    /// </summary>
    Justify = 4
}