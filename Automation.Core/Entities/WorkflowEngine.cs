using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// موتور اجرای فرآیندها
/// Workflow engine execution
/// </summary>
public class WorkflowEngine : BaseEntity
{
    /// <summary>
    /// نام موتور
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تنظیمات موتور (JSON)
    /// </summary>
    public string EngineSettings { get; set; } = "{}";

    /// <summary>
    /// کلاس اجرایی موتور
    /// </summary>
    public string EngineClass { get; set; } = string.Empty;

    /// <summary>
    /// آیا موتور پیش‌فرض است؟
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// نسخه موتور
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// مسیر فایل DLL موتور
    /// </summary>
    public string? DllPath { get; set; }

    /// <summary>
    /// نمونه‌های اجرایی موتور
    /// </summary>
    public virtual ICollection<WorkflowEngineInstance> Instances { get; set; } = new List<WorkflowEngineInstance>();
}

/// <summary>
/// نمونه اجرایی موتور
/// Workflow engine instance
/// </summary>
public class WorkflowEngineInstance : BaseEntity
{
    /// <summary>
    /// شناسه موتور
    /// </summary>
    public int EngineId { get; set; }
    public virtual WorkflowEngine Engine { get; set; } = null!;

    /// <summary>
    /// شناسه فرآیند
    /// </summary>
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    /// <summary>
    /// شناسه نسخه فرآیند
    /// </summary>
    public int WorkflowVersionId { get; set; }
    public virtual WorkflowVersion WorkflowVersion { get; set; } = null!;

    /// <summary>
    /// وضعیت اجرای موتور
    /// </summary>
    public WorkflowEngineInstanceStatus Status { get; set; } = WorkflowEngineInstanceStatus.Stopped;

    /// <summary>
    /// پیکربندی موتور (JSON)
    /// </summary>
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// پارامترهای اجرایی (JSON)
    /// </summary>
    public string Parameters { get; set; } = "{}";

    /// <summary>
    /// تاریخ شروع
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// تاریخ توقف
    /// </summary>
    public DateTime? StoppedAt { get; set; }

    /// <summary>
    /// تاریخ آخرین فعالیت
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>
    /// خطاها (JSON)
    /// </summary>
    public string? Errors { get; set; }

    /// <summary>
    /// لاگ‌های اجرایی (JSON)
    /// </summary>
    public string Logs { get; set; } = "[]";

    /// <summary>
    /// آیا موتور در حال اجرا است؟
    /// </summary>
    public bool IsRunning { get; set; } = false;
}

/// <summary>
/// وضعیت نمونه اجرایی موتور
/// </summary>
public enum WorkflowEngineInstanceStatus
{
    /// <summary>
    /// متوقف شده
    /// </summary>
    Stopped = 1,

    /// <summary>
    /// در حال اجرا
    /// </summary>
    Running = 2,

    /// <summary>
    /// در حال راه‌اندازی
    /// </summary>
    Starting = 3,

    /// <summary>
    /// در حال توقف
    /// </summary>
    Stopping = 4,

    /// <summary>
    /// خطا
    /// </summary>
    Error = 5,

    /// <summary>
    /// متوقف شده به‌طور خودکار
    /// </summary>
    AutoStopped = 6
}