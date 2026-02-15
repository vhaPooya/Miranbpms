namespace Automation.Core.Entities;

/// <summary>
/// کاربران سیستم
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// عنوان (آقای، خانم، دکتر، مهندس و...)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// نام کاربری
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// رمز عبور (Hash شده)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// نام
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// نام خانوادگی
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// نام پدر
    /// </summary>
    public string? FatherName { get; set; }

    /// <summary>
    /// نام کامل
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// کد ملی
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// جنسیت (male/female)
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// وضعیت تاهل (single/married)
    /// </summary>
    public string? MaritalStatus { get; set; }

    /// <summary>
    /// شماره پرسنلی
    /// </summary>
    public string? PersonnelNumber { get; set; }

    /// <summary>
    /// ایمیل
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// شماره موبایل
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// تلفن ثابت
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// آدرس محل سکونت
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// شهرستان
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// استان
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// مسیر تصویر کاربر
    /// </summary>
    public string? AvatarPath { get; set; }

    /// <summary>
    /// آیا کاربر آنلاین است
    /// </summary>
    public bool IsOnline { get; set; } = false;

    /// <summary>
    /// آخرین زمان ورود
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// آخرین تغییر رمز عبور
    /// </summary>
    public DateTime? LastPasswordChange { get; set; }

    /// <summary>
    /// تاریخ حذف (Soft delete)
    /// </summary>
    public DateTime? Deleted { get; set; }

    /// <summary>
    /// تاریخ ایجاد (همان CreationDate - سازگاری)
    /// </summary>
    public DateTime Created { get => CreationDate; set => CreationDate = value; }

    /// <summary>
    /// تاریخ ویرایش (همان EditDate - سازگاری)
    /// </summary>
    public DateTime? Modified { get => EditDate; set => EditDate = value; }

    /// <summary>
    /// شناسه بخش/واحد سازمانی
    /// </summary>
    public int? DepartmentId { get; set; }
    public virtual Department? Department { get; set; }

    /// <summary>
    /// شناسه سازمان
    /// </summary>
    public int? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }

    /// <summary>
    /// نقش‌های کاربر
    /// </summary>
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// ??????? ??????? ?????
    /// </summary>
    public virtual ICollection<UserPosition> UserPositions { get; set; } = new List<UserPosition>();

    /// <summary>
    /// امضاهای کاربر
    /// </summary>
    public virtual ICollection<UserSignature> UserSignatures { get; set; } = new List<UserSignature>();
}




