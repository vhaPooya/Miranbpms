namespace Automation.Core.Entities;

/// <summary>
/// توابع اسکالر تعریف شده توسط کاربر
/// User-defined scalar functions for field default values
/// </summary>
public class ScalarFunction : BaseEntity
{
    /// <summary>
    /// نام تابع (سیستمی)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی فارسی
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی انگلیسی
    /// </summary>
    public string? DisplayNameEn { get; set; }

    /// <summary>
    /// نوع خروجی تابع (nvarchar, int, datetime, bit, decimal)
    /// </summary>
    public SqlDataType ReturnType { get; set; }

    /// <summary>
    /// بدنه تابع SQL یا عبارت
    /// </summary>
    public string FunctionBody { get; set; } = string.Empty;

    /// <summary>
    /// پارامترهای تابع (JSON)
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// دسته‌بندی تابع
    /// </summary>
    public FunctionCategory Category { get; set; } = FunctionCategory.General;

    /// <summary>
    /// آیا تابع سیستمی است
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// فیلدهایی که از این تابع استفاده می‌کنند
    /// </summary>
    public virtual ICollection<FormField> FormFields { get; set; } = new List<FormField>();
}

/// <summary>
/// انواع داده SQL Server
/// </summary>
public enum SqlDataType
{
    NVarChar = 1,
    Int = 2,
    BigInt = 3,
    Bit = 4,
    DateTime = 5,
    Date = 6,
    Time = 7,
    Decimal = 8,
    Float = 9,
    UniqueIdentifier = 10,
    NVarCharMax = 11
}

/// <summary>
/// دسته‌بندی توابع
/// </summary>
public enum FunctionCategory
{
    /// <summary>
    /// عمومی
    /// </summary>
    General = 1,

    /// <summary>
    /// تاریخ و زمان
    /// </summary>
    DateTime = 2,

    /// <summary>
    /// متنی
    /// </summary>
    Text = 3,

    /// <summary>
    /// ریاضی
    /// </summary>
    Math = 4,

    /// <summary>
    /// سیستمی
    /// </summary>
    System = 5
}


