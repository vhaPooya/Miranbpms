namespace Automation.Core.Entities;

/// <summary>
/// امضاهای کاربران
/// </summary>
public class UserSignature : BaseEntity
{
    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }
    public virtual User? User { get; set; }

    /// <summary>
    /// عنوان امضاء
    /// </summary>
    public string SignatureTitle { get; set; } = string.Empty;

    /// <summary>
    /// مسیر فایل امضاء
    /// </summary>
    public string SignaturePath { get; set; } = string.Empty;

    /// <summary>
    /// آیا این امضاء پیش‌فرض است
    /// </summary>
    public bool IsDefault { get; set; } = false;
}


