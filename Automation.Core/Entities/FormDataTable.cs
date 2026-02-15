namespace Automation.Core.Entities;

/// <summary>
/// اطلاعات جدول دیتابیس ایجاد شده برای هر فرم
/// </summary>
public class FormDataTable : BaseEntity
{
    /// <summary>
    /// شناسه فرم
    /// </summary>
    public int FormId { get; set; }
    public virtual Form Form { get; set; } = null!;
    
    /// <summary>
    /// نام جدول در دیتابیس
    /// </summary>
    public string TableName { get; set; } = string.Empty;
    
    /// <summary>
    /// آیا جدول ایجاد شده است؟
    /// </summary>
    public bool IsTableCreated { get; set; } = false;
    
    /// <summary>
    /// تاریخ ایجاد جدول
    /// </summary>
    public DateTime? TableCreatedDate { get; set; }
    
    /// <summary>
    /// SQL Schema جدول
    /// </summary>
    public string? TableSchema { get; set; }
}



