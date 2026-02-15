using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.Controllers;

/// <summary>
/// Controller for Referred (کارتابل ارجاعی)
/// </summary>
public class ReferredController : Controller
{
    private readonly IDapperService _dapperService;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<ReferredController> _logger;

    public ReferredController(IDapperService dapperService, IUserContextService userContextService, ILogger<ReferredController> logger)
    {
        _dapperService = dapperService;
        _userContextService = userContextService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه اصلی کارتابل ارجاعی
    /// </summary>
    [HttpGet]
    [Route("referred")]
    public IActionResult Index()
    {
        return PartialView("Index");
    }

    /// <summary>
    /// دریافت لیست ارجاعات
    /// </summary>
    [HttpGet]
    [Route("api/referred")]
    public async Task<IActionResult> GetReferred([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var userId = _userContextService.GetCurrentUserId();

        if (!userId.HasValue)
        {
            return Json(new { success = false, error = "کاربر وارد نشده است" });
        }

        try
        {
            var result = await GetReferredDataAsync(userId.Value, page, pageSize, search, status);
            return Json(new { success = true, data = result.Items, totalCount = result.Total, page, pageSize, statusCounts = result.StatusCounts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching referred data");
            return Json(new { success = false, error = "خطا در دریافت اطلاعات" });
        }
    }

    private async Task<ReferredResult> GetReferredDataAsync(int userId, int page, int pageSize, string? search, string? status)
    {
        var offset = (page - 1) * pageSize;

        var sql = @"
            SELECT
                d.Id as DocumentId,
                d.DocumentNumber,
                d.Subject,
                dr.ReferralDateTime as ReferralDate,
                dt.IsViewed,
                dt.IsReviewed,
                u.FirstName + ' ' + u.LastName as RecipientName,
                u.Id as RecipientId,
                dept.DepartmentName as RecipientDepartment,
                urg.PriorityLevel as Priority,
                conf.Level as ConfidentialityLevel,
                dr.Status as ReferralStatus
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            LEFT JOIN DocumentTrackings dt ON d.Id = dt.DocumentId AND dt.UserId = dr.RecipientUserId
            LEFT JOIN Users u ON dr.RecipientUserId = u.Id
            LEFT JOIN Departments dept ON u.DepartmentId = dept.Id
            LEFT JOIN Urgencies urg ON d.UrgencyId = urg.Id
            LEFT JOIN ConfidentialityLevels conf ON d.ConfidentialityLevelId = conf.Id
            WHERE d.IsDeleted = 0
            AND dr.ReferrerUserId = @UserId";

        object parameters;

        if (!string.IsNullOrEmpty(status) && !string.IsNullOrEmpty(search))
        {
            sql += " AND dr.Status = @Status AND (d.Subject LIKE @Search OR d.DocumentNumber LIKE @Search)";
            parameters = new { UserId = userId, Status = status, Search = $"%{search}%", Offset = offset, PageSize = pageSize };
        }
        else if (!string.IsNullOrEmpty(status))
        {
            sql += " AND dr.Status = @Status";
            parameters = new { UserId = userId, Status = status, Offset = offset, PageSize = pageSize };
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
            ORDER BY dr.ReferralDateTime DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var countSql = @"
            SELECT COUNT(DISTINCT d.Id)
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            WHERE d.IsDeleted = 0
            AND dr.ReferrerUserId = @UserId";

        if (!string.IsNullOrEmpty(status))
            countSql += " AND dr.Status = @Status";
        if (!string.IsNullOrEmpty(search))
            countSql += " AND (d.Subject LIKE @Search OR d.DocumentNumber LIKE @Search)";

        var items = await _dapperService.QueryAsync<ReferredItemDto>(sql, parameters);
        var total = await _dapperService.QueryFirstOrDefaultAsync<int>(countSql, parameters);
        var statusCounts = await GetStatusCountsAsync(userId);

        return new ReferredResult
        {
            Items = items.ToList(),
            Total = total,
            StatusCounts = statusCounts
        };
    }

    private async Task<Dictionary<string, int>> GetStatusCountsAsync(int userId)
    {
        const string sql = @"
            SELECT
                dr.Status,
                COUNT(DISTINCT d.Id) as Count
            FROM Documents d
            INNER JOIN DocumentReferrals dr ON d.Id = dr.DocumentId
            WHERE d.IsDeleted = 0
            AND dr.ReferrerUserId = @UserId
            GROUP BY dr.Status";

        var results = await _dapperService.QueryAsync<StatusCountDto>(sql, new { UserId = userId });
        return results.ToDictionary(x => x.Status ?? "بدون وضعیت", x => x.Count);
    }
}

#region DTOs

public class ReferredResult
{
    public List<ReferredItemDto> Items { get; set; } = new();
    public int Total { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}

public class ReferredItemDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime ReferralDate { get; set; }
    public bool IsViewed { get; set; }
    public bool IsReviewed { get; set; }
    public string? RecipientName { get; set; }
    public int? RecipientId { get; set; }
    public string? RecipientDepartment { get; set; }
    public string? Priority { get; set; }
    public string? ConfidentialityLevel { get; set; }
    public string? ReferralStatus { get; set; }
}

public class StatusCountDto
{
    public string? Status { get; set; }
    public int Count { get; set; }
}

#endregion


