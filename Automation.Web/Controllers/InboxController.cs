using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.Controllers;

/// <summary>
/// Controller for Inbox (کارتابل وارده)
/// </summary>
public class InboxController : Controller
{
    private readonly IDapperService _dapperService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<InboxController> _logger;

    public InboxController(IDapperService dapperService, IUserContextService userContextService, ILogger<InboxController> logger)
    {
        _dapperService = dapperService;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه اصلی کارتابل وارده
    /// </summary>
    [HttpGet]
    [Route("inbox")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    /// <summary>
    /// دریافت لیست نامه‌های وارده
    /// </summary>
    [HttpGet]
    [Route("api/inbox")]
    public async Task<IActionResult> GetInbox([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? actionType = null)
    {
        var userId = _userContextService.GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Json(new { success = false, error = "کاربر وارد نشده است" });
        }

        try
        {
            var result = await GetInboxDataAsync(userId.Value, page, pageSize, search, actionType);
            return Json(new { success = true, data = result.Items, totalCount = result.Total, page, pageSize, actionTypeCounts = result.ActionTypeCounts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching inbox data");
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }

    /// <summary>
    /// دریافت تعداد بر اساس نوع اقدام
    /// </summary>
    [HttpGet]
    [Route("api/inbox/action-counts")]
    public async Task<IActionResult> GetActionCounts()
    {
        var userId = _userContextService.GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Json(new { success = false, error = "کاربر وارد نشده است" });
        }

        try
        {
            var counts = await GetActionTypeCountsAsync(userId.Value);
            return Json(new { success = true, data = counts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching action counts");
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }

    /// <summary>
    /// تغییر وضعیت خوانده شده
    /// </summary>
    [HttpPost]
    [Route("api/inbox/read-status")]
    public async Task<IActionResult> SetReadStatus([FromBody] ReadStatusDto dto)
    {
        var userId = _userContextService.GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Json(new { success = false, error = "کاربر وارد نشده است" });
        }

        try
        {
            var sql = @"
                UPDATE DocumentTrackings
                SET IsViewed = @IsViewed, ViewedAt = @ViewedAt
                WHERE DocumentId = @DocumentId AND UserId = @UserId";

            await _dapperService.ExecuteAsync(sql, new
            {
                DocumentId = dto.DocumentId,
                UserId = userId.Value,
                IsViewed = dto.IsRead,
                ViewedAt = dto.IsRead ? DateTime.UtcNow : (DateTime?)null
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating read status");
            return Json(new { success = false, error = "خطا در بروزرسانی وضعیت" });
        }
    }

    private async Task<InboxResult> GetInboxDataAsync(int userId, int page, int pageSize, string? search, string? actionType)
    {
        var offset = (page - 1) * pageSize;

        var sql = @"
            SELECT
                d.Id as DocumentId,
                d.DocumentNumber,
                d.Subject,
                d.CreatedDateTime as ReceivedDateTime,
                dt.IsViewed,
                dt.IsReviewed,
                u.FirstName + ' ' + u.LastName as SenderName,
                u.Id as SenderId,
                urg.PriorityLevel as Priority,
                conf.Level as ConfidentialityLevel,
                (SELECT COUNT(*) FROM DocumentAttachments da WHERE da.DocumentId = d.Id) as AttachmentCount,
                at.ActionNameFa as ActionTypeName,
                at.Id as ActionTypeId
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            LEFT JOIN DocumentTrackings dt ON d.Id = dt.DocumentId AND dt.UserId = @UserId
            LEFT JOIN Users u ON d.CreatedByUserId = u.Id
            LEFT JOIN Urgencies urg ON d.UrgencyId = urg.Id
            LEFT JOIN ConfidentialityLevels conf ON d.ConfidentialityLevelId = conf.Id
            LEFT JOIN ActionTypes at ON dr.ActionTypeId = at.Id
            WHERE d.IsDeleted = 0
            AND dr.RecipientUserId = @UserId";

        object parameters;

        if (!string.IsNullOrEmpty(actionType) && !string.IsNullOrEmpty(search))
        {
            sql += " AND at.ActionNameFa = @ActionType AND (d.Subject LIKE @Search OR d.DocumentNumber LIKE @Search)";
            parameters = new { UserId = userId, ActionType = actionType, Search = $"%{search}%", Offset = offset, PageSize = pageSize };
        }
        else if (!string.IsNullOrEmpty(actionType))
        {
            sql += " AND at.ActionNameFa = @ActionType";
            parameters = new { UserId = userId, ActionType = actionType, Offset = offset, PageSize = pageSize };
        }
        else if (!string.IsNullOrEmpty(search))
        {
            sql += " AND (d.Subject LIKE @Search OR d.DocumentNumber LIKE @Search)";
            parameters = new { UserId = userId, Search = $"%{search}%", Offset = offset, PageSize = pageSize };
        }
        else
        {
            parameters = new { UserId = userId, Offset = offset, PageSize = pageSize };
        }

        sql += @"
            ORDER BY d.CreatedDateTime DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var countSql = @"
            SELECT COUNT(DISTINCT d.Id)
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            LEFT JOIN ActionTypes at ON dr.ActionTypeId = at.Id
            WHERE d.IsDeleted = 0
            AND dr.RecipientUserId = @UserId";

        if (!string.IsNullOrEmpty(actionType))
            countSql += " AND at.ActionNameFa = @ActionType";
        if (!string.IsNullOrEmpty(search))
            countSql += " AND (d.Subject LIKE @Search OR d.DocumentNumber LIKE @Search)";

        var items = await _dapperService.QueryAsync<InboxItemDto>(sql, parameters);
        var total = await _dapperService.QueryFirstOrDefaultAsync<int>(countSql, parameters);
        var actionTypeCounts = await GetActionTypeCountsAsync(userId);

        return new InboxResult
        {
            Items = items.ToList(),
            Total = total,
            ActionTypeCounts = actionTypeCounts
        };
    }

    private async Task<Dictionary<string, int>> GetActionTypeCountsAsync(int userId)
    {
        const string sql = @"
            SELECT
                at.ActionNameFa as ActionType,
                COUNT(DISTINCT d.Id) as Count
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            LEFT JOIN ActionTypes at ON dr.ActionTypeId = at.Id
            WHERE d.IsDeleted = 0
            AND dr.RecipientUserId = @UserId
            GROUP BY at.ActionNameFa";

        var results = await _dapperService.QueryAsync<ActionTypeCountDto>(sql, new { UserId = userId });
        return results.ToDictionary(x => x.ActionType ?? "بدون نوع", x => x.Count);
    }
}

#region DTOs

public class InboxResult
{
    public List<InboxItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public Dictionary<string, int> ActionTypeCounts { get; set; } = new();
}

public class InboxItemDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime ReceivedDateTime { get; set; }
    public bool IsViewed { get; set; }
    public bool IsReviewed { get; set; }
    public string? SenderName { get; set; }
    public int? SenderId { get; set; }
    public string? Priority { get; set; }
    public string? ConfidentialityLevel { get; set; }
    public int AttachmentCount { get; set; }
    public string? ActionTypeName { get; set; }
    public int? ActionTypeId { get; set; }
}

public class ActionTypeCountDto
{
    public string? ActionType { get; set; }
    public int Count { get; set; }
}

public class ReadStatusDto
{
    public int DocumentId { get; set; }
    public bool IsRead { get; set; }
}

#endregion


