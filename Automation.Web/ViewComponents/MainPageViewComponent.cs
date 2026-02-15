using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.ViewComponents;

/// <summary>
/// ViewComponent برای صفحه اصلی با داده‌های داینامیک
/// </summary>
public class MainPageViewComponent : ViewComponent
{
    private readonly IDapperService _dapperService;
    private readonly IUserContextService _userContextService;

    public MainPageViewComponent(IDapperService dapperService, IUserContextService userContextService)
    {
        _dapperService = dapperService;
        _userContextService = userContextService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userContextService.GetCurrentUserId();
        
        if (!userId.HasValue)
        {
            return View(new MainPageViewModel());
        }

        var viewModel = new MainPageViewModel
        {
            UnreadCount = await GetUnreadCountAsync(userId.Value),
            UrgentCount = await GetUrgentCountAsync(userId.Value),
            PendingApprovalCount = await GetPendingApprovalCountAsync(userId.Value),
            TodayTasks = await GetTodayTasksAsync(userId.Value),
            MonthlyPerformance = await GetMonthlyPerformanceAsync(userId.Value),
            SystemNews = await GetSystemNewsAsync(),
            CurrentDate = GetPersianDate()
        };

        return View(viewModel);
    }

    private async Task<int> GetUnreadCountAsync(int userId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT d.Id)
            FROM Documents d
            LEFT JOIN DocumentTrackings dt ON d.Id = dt.DocumentId AND dt.UserId = @UserId
            WHERE d.IsDeleted = 0
            AND (dt.IsViewed = 0 OR dt.IsViewed IS NULL)
            AND EXISTS (
                SELECT 1 FROM DocumentReferrals dr 
                WHERE dr.DocumentId = d.Id AND dr.RecipientUserId = @UserId
            )";

        var result = await _dapperService.QueryFirstOrDefaultAsync<int?>(sql, new { UserId = userId });
        return result ?? 0;
    }

    private async Task<int> GetUrgentCountAsync(int userId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT d.Id)
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            INNER JOIN Urgencies u ON d.UrgencyId = u.Id
            WHERE d.IsDeleted = 0
            AND dr.RecipientUserId = @UserId
            AND u.PriorityLevel IN ('URGENT', 'VERY_URGENT')";

        var result = await _dapperService.QueryFirstOrDefaultAsync<int?>(sql, new { UserId = userId });
        return result ?? 0;
    }

    private async Task<int> GetPendingApprovalCountAsync(int userId)
    {
        // Workflow removed for re-design; keep placeholder until new engine is implemented.
        await Task.CompletedTask;
        return 0;
    }

    private async Task<List<MainPageTaskItem>> GetTodayTasksAsync(int userId)
    {
        const string sql = @"
            SELECT TOP 5
                d.Id as TaskId,
                d.Subject as Title,
                CASE WHEN dt.IsReviewed = 1 THEN 1 ELSE 0 END as IsCompleted
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            LEFT JOIN DocumentTrackings dt ON d.Id = dt.DocumentId AND dt.UserId = @UserId
            WHERE d.IsDeleted = 0
            AND dr.RecipientUserId = @UserId
            AND CAST(d.CreatedDateTime AS DATE) = CAST(GETDATE() AS DATE)
            ORDER BY d.CreatedDateTime DESC";

        var tasks = await _dapperService.QueryAsync<MainPageTaskItem>(sql, new { UserId = userId });
        return tasks.ToList();
    }

    private async Task<int> GetMonthlyPerformanceAsync(int userId)
    {
        const string sql = @"
            SELECT 
                CAST(COUNT(CASE WHEN dt.IsReviewed = 1 THEN 1 END) * 100.0 / NULLIF(COUNT(*), 0) AS INT) as Percentage
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            LEFT JOIN DocumentTrackings dt ON d.Id = dt.DocumentId AND dt.UserId = @UserId
            WHERE d.IsDeleted = 0
            AND dr.RecipientUserId = @UserId
            AND MONTH(d.CreatedDateTime) = MONTH(GETDATE())
            AND YEAR(d.CreatedDateTime) = YEAR(GETDATE())";

        var result = await _dapperService.QueryFirstOrDefaultAsync<int?>(sql, new { UserId = userId });
        return result ?? 0;
    }

    private async Task<string> GetSystemNewsAsync()
    {
        const string sql = @"
            SELECT TOP 1 SettingValue as Value
            FROM SystemSettings
            WHERE SettingKey = 'SystemNews'
            ORDER BY UpdatedAt DESC";

        return await _dapperService.QueryFirstOrDefaultAsync<string>(sql) ?? "نسخه جدید با موفقیت آپدیت شد.";
    }

    private string GetPersianDate()
    {
        // استفاده از کتابخانه تاریخ شمسی یا تبدیل ساده
        var now = DateTime.Now;
        var persianCalendar = new System.Globalization.PersianCalendar();
        return $"{persianCalendar.GetDayOfMonth(now)} {GetPersianMonthName(persianCalendar.GetMonth(now))} {persianCalendar.GetYear(now)}";
    }

    private string GetPersianMonthName(int month)
    {
        var months = new[] { "", "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
        return months[month];
    }
}

public class MainPageViewModel
{
    public int UnreadCount { get; set; }
    public int UrgentCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public List<MainPageTaskItem> TodayTasks { get; set; } = new();
    public int MonthlyPerformance { get; set; }
    public string SystemNews { get; set; } = "نسخه جدید با موفقیت آپدیت شد.";
    public string CurrentDate { get; set; } = string.Empty;
}

public class MainPageTaskItem
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}



