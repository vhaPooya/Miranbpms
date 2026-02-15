using System.ComponentModel.DataAnnotations;

namespace Automation.Core.Entities;

/// <summary>
/// فرآیند گردش کار
/// Workflow Process
/// </summary>
public class Workflow : BaseEntity
{
    /// <summary>
    /// کد شناسه فرآیند (کلید بیزینس - 4 رقمی)
    /// </summary>
    [Range(1000, 9999, ErrorMessage = "کد فرآیند باید 4 رقمی باشد")]
    public int Code { get; set; }

    /// <summary>
    /// کد نمایشی فرآیند (مثل WFL-001)
    /// </summary>
    public string WorkflowCode { get; set; } = string.Empty;

    /// <summary>
    /// نام فارسی فرآیند
    /// </summary>
    public string NameFa { get; set; } = string.Empty;

    /// <summary>
    /// نام انگلیسی فرآیند
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات فرآیند
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// دسته‌بندی فرآیند
    /// </summary>
    public int? CategoryId { get; set; }
    public virtual WorkflowCategory? Category { get; set; }

    /// <summary>
    /// نسخه فرآیند
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// آیا منتشر شده
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// تاریخ انتشار
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// فرم مرتبط با فرآیند
    /// </summary>
    public int? FormId { get; set; }
    public virtual Form? Form { get; set; }

    /// <summary>
    /// داده‌های طراحی فرآیند (JSON کامل)
    /// </summary>
    public string? DesignData { get; set; }

    /// <summary>
    /// نسخه‌های فرآیند
    /// </summary>
    public virtual ICollection<WorkflowVersion> Versions { get; set; } = new List<WorkflowVersion>();

    /// <summary>
    /// نودهای فرآیند
    /// </summary>
    public virtual ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();

    /// <summary>
    /// اتصالات بین نودها
    /// </summary>
    public virtual ICollection<WorkflowConnection> Connections { get; set; } = new List<WorkflowConnection>();

    /// <summary>
    /// متغیرهای فرآیند
    /// </summary>
    public virtual ICollection<WorkflowVariable> Variables { get; set; } = new List<WorkflowVariable>();

    /// <summary>
    /// نمونه‌های اجرایی فرآیند
    /// </summary>
    public virtual ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}

/// <summary>
/// دسته‌بندی فرآیندها
/// </summary>
public class WorkflowCategory : BaseEntity
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
    public virtual WorkflowCategory? ParentCategory { get; set; }

    /// <summary>
    /// زیرمجموعه‌ها
    /// </summary>
    public virtual ICollection<WorkflowCategory> SubCategories { get; set; } = new List<WorkflowCategory>();

    /// <summary>
    /// فرآیندهای این دسته‌بندی
    /// </summary>
    public virtual ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}

/// <summary>
/// نسخه فرآیند
/// </summary>
public class WorkflowVersion : BaseEntity
{
    /// <summary>
    /// شناسه فرآیند
    /// </summary>
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    /// <summary>
    /// شماره نسخه
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// آیا نسخه فعال است؟
    /// </summary>
    public bool IsActiveVersion { get; set; } = false;

    /// <summary>
    /// داده‌های طراحی (JSON)
    /// </summary>
    public string DesignData { get; set; } = string.Empty;

    /// <summary>
    /// اسکریپت‌های سفارشی
    /// </summary>
    public string? CustomScripts { get; set; }

    /// <summary>
    /// تنظیمات فرم
    /// </summary>
    public string? FormSettings { get; set; }

    /// <summary>
    /// توضیحات نسخه
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// نود فرآیند
/// </summary>
public class WorkflowNode : BaseEntity
{
    /// <summary>
    /// شناسه فرآیند
    /// </summary>
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    /// <summary>
    /// نوع نود
    /// </summary>
    public WorkflowNodeType NodeType { get; set; }

    /// <summary>
    /// کد نوع نود به صورت رشته (BPMN: START_EVENT, TASK, USER_TASK, ...)
    /// </summary>
    public string NodeTypeCode { get; set; } = string.Empty;

    /// <summary>
    /// شناسه تعریف فرآیند (همان WorkflowId - برای سازگاری با موتور BPMN)
    /// </summary>
    public int WorkflowDefinitionId { get => WorkflowId; set => WorkflowId = value; }

    /// <summary>
    /// کلاس سرویس (برای نود سرویس)
    /// </summary>
    public string? ServiceClass { get; set; }

    /// <summary>
    /// متد سرویس
    /// </summary>
    public string? ServiceMethod { get; set; }

    /// <summary>
    /// پارامترهای سرویس (JSON)
    /// </summary>
    public string? ServiceParameters { get; set; }

    /// <summary>
    /// نوع اختصاص‌دهنده (USER, ROLE, GROUP)
    /// </summary>
    public string? AssigneeType { get; set; }

    /// <summary>
    /// شناسه اختصاص‌دهنده
    /// </summary>
    public int? AssigneeId { get; set; }

    /// <summary>
    /// مهلت انجام (برای USER_TASK)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// نام نود
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// عنوان فارسی
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// مختصات X
    /// </summary>
    public double PositionX { get; set; }

    /// <summary>
    /// مختصات Y
    /// </summary>
    public double PositionY { get; set; }

    /// <summary>
    /// عرض نود
    /// </summary>
    public double Width { get; set; } = 120;

    /// <summary>
    /// ارتفاع نود
    /// </summary>
    public double Height { get; set; } = 60;

    /// <summary>
    /// تنظیمات نود (JSON)
    /// </summary>
    public string Settings { get; set; } = "{}";

    /// <summary>
    /// اسکریپت‌های سفارشی نود
    /// </summary>
    public string? CustomScripts { get; set; }

    /// <summary>
    /// شرط نمایش نود
    /// </summary>
    public string? VisibilityCondition { get; set; }

    /// <summary>
    /// نودهای ورودی
    /// </summary>
    public virtual ICollection<WorkflowConnection> IncomingConnections { get; set; } = new List<WorkflowConnection>();

    /// <summary>
    /// نودهای خروجی
    /// </summary>
    public virtual ICollection<WorkflowConnection> OutgoingConnections { get; set; } = new List<WorkflowConnection>();
}

/// <summary>
/// اتصال بین نودها
/// </summary>
public class WorkflowConnection : BaseEntity
{
    /// <summary>
    /// شناسه فرآیند
    /// </summary>
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    /// <summary>
    /// نود مبدا
    /// </summary>
    public int SourceNodeId { get; set; }
    public virtual WorkflowNode SourceNode { get; set; } = null!;

    /// <summary>
    /// نود مقصد
    /// </summary>
    public int TargetNodeId { get; set; }
    public virtual WorkflowNode TargetNode { get; set; } = null!;

    /// <summary>
    /// عنوان اتصال
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// شرط انتقال
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// ترتیب اتصال
    /// </summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// متغیرهای فرآیند
/// </summary>
public class WorkflowVariable : BaseEntity
{
    /// <summary>
    /// شناسه فرآیند
    /// </summary>
    public int WorkflowId { get; set; }
    public virtual Workflow Workflow { get; set; } = null!;

    /// <summary>
    /// نام متغیر
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نوع متغیر
    /// </summary>
    public WorkflowVariableType VariableType { get; set; }

    /// <summary>
    /// مقدار پیش‌فرض
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// آیا ورودی کاربر است؟
    /// </summary>
    public bool IsUserInput { get; set; } = false;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// نمونه اجرایی فرآیند
/// </summary>
public class WorkflowInstance : BaseEntity
{
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
    /// شناسه سند مرتبط
    /// </summary>
    public int? DocumentId { get; set; }

    /// <summary>
    /// شناسه فرم مرتبط
    /// </summary>
    public int? FormId { get; set; }

    /// <summary>
    /// وضعیت اجرای فرآیند
    /// </summary>
    public WorkflowInstanceState State { get; set; }

    /// <summary>
    /// تاریخ شروع
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// تاریخ پایان
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// وضعیت (رشته - برای موتور BPMN)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// تاریخ پایان (برای موتور BPMN)
    /// </summary>
    public DateTime? EndedDate { get; set; }

    /// <summary>
    /// داده ورودی (JSON)
    /// </summary>
    public string? InputData { get; set; }

    /// <summary>
    /// تاریخ شروع (برای موتور BPMN)
    /// </summary>
    public DateTime? StartedDate { get; set; }

    /// <summary>
    /// شناسه کاربر شروع‌کننده
    /// </summary>
    public int StartedByUserId { get; set; }

    /// <summary>
    /// شناسه تعریف فرآیند (همان WorkflowId - برای سازگاری با موتور BPMN)
    /// </summary>
    public int WorkflowDefinitionId { get => WorkflowId; set => WorkflowId = value; }

    /// <summary>
    /// تعریف فرآیند (همان Workflow - برای سازگاری با موتور BPMN)
    /// </summary>
    public virtual Workflow WorkflowDefinition { get => Workflow; set => Workflow = value; }

    /// <summary>
    /// کاربر شروع‌کننده (همان StartedByUserId - برای سازگاری با موتور BPMN)
    /// </summary>
    public int StarterUserId { get => StartedByUserId; set => StartedByUserId = value; }

    /// <summary>
    /// مقادیر متغیرها
    /// </summary>
    public virtual ICollection<WorkflowInstanceVariable> Variables { get; set; } = new List<WorkflowInstanceVariable>();

    /// <summary>
    /// گردش اجرای فرآیند
    /// </summary>
    public virtual ICollection<WorkflowInstanceTransition> Transitions { get; set; } = new List<WorkflowInstanceTransition>();

    /// <summary>
    /// توکن‌های فعال (برای موتور BPMN)
    /// </summary>
    public virtual ICollection<WorkflowToken> CurrentTokens { get; set; } = new List<WorkflowToken>();
}

/// <summary>
/// مقدار متغیر در نمونه فرآیند
/// </summary>
public class WorkflowInstanceVariable : BaseEntity
{
    /// <summary>
    /// شناسه نمونه فرآیند
    /// </summary>
    public int WorkflowInstanceId { get; set; }
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;

    /// <summary>
    /// شناسه متغیر
    /// </summary>
    public int VariableId { get; set; }
    public virtual WorkflowVariable Variable { get; set; } = null!;

    /// <summary>
    /// مقدار متغیر
    /// </summary>
    public string? Value { get; set; }
}

/// <summary>
/// گردش اجرای فرآیند
/// </summary>
public class WorkflowInstanceTransition : BaseEntity
{
    /// <summary>
    /// شناسه نمونه فرآیند
    /// </summary>
    public int WorkflowInstanceId { get; set; }
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;

    /// <summary>
    /// نود مبدا
    /// </summary>
    public int? SourceNodeId { get; set; }
    public virtual WorkflowNode? SourceNode { get; set; }

    /// <summary>
    /// نود مقصد
    /// </summary>
    public int? TargetNodeId { get; set; }
    public virtual WorkflowNode? TargetNode { get; set; }

    /// <summary>
    /// شناسه کاربر انجام‌دهنده
    /// </summary>
    public int? PerformedByUserId { get; set; }

    /// <summary>
    /// زمان انجام
    /// </summary>
    public DateTime PerformedAt { get; set; }

    /// <summary>
    /// نتیجه انتقال
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// انواع نودهای فرآیند
/// </summary>
public enum WorkflowNodeType
{
    /// <summary>
    /// نود شروع
    /// </summary>
    Start = 1,

    /// <summary>
    /// نود پایان
    /// </summary>
    End = 2,

    /// <summary>
    /// نود تسک (وظیفه)
    /// </summary>
    Task = 3,

    /// <summary>
    /// نود تصمیم‌گیری
    /// </summary>
    Decision = 4,

    /// <summary>
    /// نود موازی
    /// </summary>
    Parallel = 5,

    /// <summary>
    /// نود ادغام
    /// </summary>
    Merge = 6,

    /// <summary>
    /// نود انتظار
    /// </summary>
    Wait = 7,

    /// <summary>
    /// نود فراخوانی سرویس
    /// </summary>
    Service = 8,

    /// <summary>
    /// نود اسکریپت
    /// </summary>
    Script = 9,

    /// <summary>
    /// نود انسانی
    /// </summary>
    Human = 10
}

/// <summary>
/// انواع متغیرها
/// </summary>
public enum WorkflowVariableType
{
    /// <summary>
    /// رشته‌ای
    /// </summary>
    String = 1,

    /// <summary>
    /// عدد صحیح
    /// </summary>
    Integer = 2,

    /// <summary>
    /// عدد اعشاری
    /// </summary>
    Decimal = 3,

    /// <summary>
    /// بولی
    /// </summary>
    Boolean = 4,

    /// <summary>
    /// تاریخ
    /// </summary>
    DateTime = 5,

    /// <summary>
    /// آرایه
    /// </summary>
    Array = 6,

    /// <summary>
    /// شیء
    /// </summary>
    Object = 7
}

/// <summary>
/// وضعیت اجرای فرآیند
/// </summary>
public enum WorkflowInstanceState
{
    /// <summary>
    /// در حال اجرا
    /// </summary>
    Running = 1,

    /// <summary>
    /// تکمیل شده
    /// </summary>
    Completed = 2,

    /// <summary>
    /// لغو شده
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// متوقف شده
    /// </summary>
    Suspended = 4,

    /// <summary>
    /// خطا
    /// </summary>
    Error = 5
}