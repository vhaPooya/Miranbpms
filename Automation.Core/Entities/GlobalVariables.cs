namespace Automation.Core.Entities;

/// <summary>
/// متغیرهای سراسری برای استفاده در فرآیندها و گزارشات
/// Global variables for use in workflows and reports
/// </summary>
public static class GlobalVariables
{
    /// <summary>
    /// شناسه کاربر جاری
    /// Current user ID
    /// </summary>
    public const string CurrentUserId = "{{CURRENT_USER_ID}}";

    /// <summary>
    /// کد پرسنلی کاربر جاری
    /// Current user personnel code
    /// </summary>
    public const string CurrentUserPersonnelCode = "{{CURRENT_USER_PERSONNEL_CODE}}";

    /// <summary>
    /// شناسه نقش کاربر جاری
    /// Current user role ID
    /// </summary>
    public const string CurrentUserRole = "{{CURRENT_USER_ROLE}}";

    /// <summary>
    /// شناسه سمت کاربر جاری
    /// Current user position ID
    /// </summary>
    public const string CurrentUserPosition = "{{CURRENT_USER_POSITION}}";

    /// <summary>
    /// شناسه دپارتمان کاربر جاری
    /// Current user department ID
    /// </summary>
    public const string CurrentUserDepartment = "{{CURRENT_USER_DEPARTMENT}}";

    /// <summary>
    /// شناسه سازمان کاربر جاری
    /// Current user organization ID
    /// </summary>
    public const string CurrentUserOrganization = "{{CURRENT_USER_ORGANIZATION}}";

    /// <summary>
    /// شناسه فرم جاری
    /// Current form ID
    /// </summary>
    public const string CurrentFormId = "{{CURRENT_FORM_ID}}";

    /// <summary>
    /// شناسه رکورد جاری
    /// Current record ID
    /// </summary>
    public const string CurrentRecordId = "{{CURRENT_RECORD_ID}}";

    /// <summary>
    /// شناسه فرآیند جاری
    /// Current workflow ID
    /// </summary>
    public const string CurrentWorkflowId = "{{CURRENT_WORKFLOW_ID}}";

    /// <summary>
    /// شناسه نمونه فرآیند جاری
    /// Current workflow instance ID
    /// </summary>
    public const string CurrentWorkflowInstanceId = "{{CURRENT_WORKFLOW_INSTANCE_ID}}";

    /// <summary>
    /// تاریخ جاری (شمسی)
    /// Current Persian date
    /// </summary>
    public const string CurrentPersianDate = "{{CURRENT_PERSIAN_DATE}}";

    /// <summary>
    /// تاریخ جاری (میلادی)
    /// Current Gregorian date
    /// </summary>
    public const string CurrentGregorianDate = "{{CURRENT_GREGORIAN_DATE}}";

    /// <summary>
    /// تاریخ جاری (هجری)
    /// Current Hijri date
    /// </summary>
    public const string CurrentHijriDate = "{{CURRENT_HIJRI_DATE}}";

    /// <summary>
    /// سال جاری (شمسی)
    /// Current Persian year
    /// </summary>
    public const string CurrentPersianYear = "{{CURRENT_PERSIAN_YEAR}}";

    /// <summary>
    /// ماه جاری (شمسی)
    /// Current Persian month
    /// </summary>
    public const string CurrentPersianMonth = "{{CURRENT_PERSIAN_MONTH}}";

    /// <summary>
    /// روز جاری (شمسی)
    /// Current Persian day
    /// </summary>
    public const string CurrentPersianDay = "{{CURRENT_PERSIAN_DAY}}";

    /// <summary>
    /// زمان جاری
    /// Current time
    /// </summary>
    public const string CurrentTime = "{{CURRENT_TIME}}";

    /// <summary>
    /// شناسه دبیرخانه جاری
    /// Current secretariat ID
    /// </summary>
    public const string CurrentSecretariatId = "{{CURRENT_SECRETARIAT_ID}}";

    /// <summary>
    /// شناسه کاربر شروع‌کننده فرآیند
    /// Process starter user ID
    /// </summary>
    public const string ProcessStarterUserId = "{{PROCESS_STARTER_USER_ID}}";

    /// <summary>
    /// شناسه نقش کاربر شروع‌کننده فرآیند
    /// Process starter user role ID
    /// </summary>
    public const string ProcessStarterUserRole = "{{PROCESS_STARTER_USER_ROLE}}";

    /// <summary>
    /// شناسه سمت کاربر شروع‌کننده فرآیند
    /// Process starter user position ID
    /// </summary>
    public const string ProcessStarterUserPosition = "{{PROCESS_STARTER_USER_POSITION}}";

    /// <summary>
    /// تاریخ شروع فرآیند
    /// Process start date
    /// </summary>
    public const string ProcessStartDate = "{{PROCESS_START_DATE}}";

    /// <summary>
    /// شماره سند جاری
    /// Current document number
    /// </summary>
    public const string CurrentDocumentNumber = "{{CURRENT_DOCUMENT_NUMBER}}";

    /// <summary>
    /// نوع سند جاری
    /// Current document type
    /// </summary>
    public const string CurrentDocumentType = "{{CURRENT_DOCUMENT_TYPE}}";

    /// <summary>
    /// شناسه سند مرجع
    /// Reference document ID
    /// </summary>
    public const string ReferenceDocumentId = "{{REFERENCE_DOCUMENT_ID}}";

    /// <summary>
    /// آدرس IP کاربر جاری
    /// Current user IP address
    /// </summary>
    public const string CurrentUserIp = "{{CURRENT_USER_IP}}";

    /// <summary>
    /// نام مرورگر کاربر جاری
    /// Current user browser name
    /// </summary>
    public const string CurrentUserBrowser = "{{CURRENT_USER_BROWSER}}";

    /// <summary>
    /// نسخه سیستم عامل کاربر جاری
    /// Current user OS version
    /// </summary>
    public const string CurrentUserOS = "{{CURRENT_USER_OS}}";

    /// <summary>
    /// شناسه نشست کاربر جاری
    /// Current user session ID
    /// </summary>
    public const string CurrentUserSessionId = "{{CURRENT_USER_SESSION_ID}}";

    /// <summary>
    /// کد یکتای کاربر جاری
    /// Current user unique code
    /// </summary>
    public const string CurrentUserUniqueCode = "{{CURRENT_USER_UNIQUE_CODE}}";

    /// <summary>
    /// کد یکتای سازمان جاری
    /// Current organization unique code
    /// </summary>
    public const string CurrentOrganizationUniqueCode = "{{CURRENT_ORGANIZATION_UNIQUE_CODE}}";

    /// <summary>
    /// کد یکتای دپارتمان جاری
    /// Current department unique code
    /// </summary>
    public const string CurrentDepartmentUniqueCode = "{{CURRENT_DEPARTMENT_UNIQUE_CODE}}";

    /// <summary>
    /// کد یکتای دبیرخانه جاری
    /// Current secretariat unique code
    /// </summary>
    public const string CurrentSecretariatUniqueCode = "{{CURRENT_SECRETARIAT_UNIQUE_CODE}}";

    /// <summary>
    /// کد یکتای فرم جاری
    /// Current form unique code
    /// </summary>
    public const string CurrentFormUniqueCode = "{{CURRENT_FORM_UNIQUE_CODE}}";

    /// <summary>
    /// کد یکتای فرآیند جاری
    /// Current workflow unique code
    /// </summary>
    public const string CurrentWorkflowUniqueCode = "{{CURRENT_WORKFLOW_UNIQUE_CODE}}";
}