namespace Automation.Core.Entities;

/// <summary>
/// دسته‌بندی فرم‌ها
/// Form Categories for organizing forms
/// </summary>
public class FormCategory : BaseEntity
{
    /// <summary>
    /// کد دسته‌بندی
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// نام فارسی
    /// </summary>
    public string NameFa { get; set; } = string.Empty;

    /// <summary>
    /// نام انگلیسی
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// آیکون دسته‌بندی
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// دسته‌بندی والد
    /// </summary>
    public int? ParentCategoryId { get; set; }
    public virtual FormCategory? ParentCategory { get; set; }

    /// <summary>
    /// زیر دسته‌ها
    /// </summary>
    public virtual ICollection<FormCategory> SubCategories { get; set; } = new List<FormCategory>();

    /// <summary>
    /// فرم‌های این دسته
    /// </summary>
    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
}


