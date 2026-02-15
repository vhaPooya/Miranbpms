using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// آیتم‌های لیست کشویی (کامبوباکس)
/// Combobox Items
/// </summary>
public class ComboboxItem : BaseEntity
{
    /// <summary>
    /// شناسه والد (برای لیست‌های سلسله‌مرتبی)
    /// </summary>
    public int? ParentId { get; set; }
    public virtual ComboboxItem? Parent { get; set; }

    /// <summary>
    /// عنوان آیتم
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TitleItem { get; set; } = string.Empty;

    /// <summary>
    /// مقدار آیتم (Value)
    /// </summary>
    [MaxLength(100)]
    public string? ValueItem { get; set; }

    /// <summary>
    /// کد گروه (برای دسته‌بندی لیست‌ها)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string GroupCode { get; set; } = string.Empty;

    /// <summary>
    /// آیا نمایش داده شود؟
    /// </summary>
    public bool IsShow { get; set; } = true;

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int SortItem { get; set; } = 0;

    /// <summary>
    /// آیتم‌های فرزند
    /// </summary>
    public virtual ICollection<ComboboxItem> Children { get; set; } = new List<ComboboxItem>();
}


