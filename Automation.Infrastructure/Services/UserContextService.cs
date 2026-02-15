using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس برای دریافت اطلاعات کاربر جاری (استفاده از IdentityDbContext برای دامنه استاتیک)
/// </summary>
public class UserContextService : IUserContextService
{
    private readonly IdentityDbContext _context;
    private readonly ISessionService? _sessionService;

    public UserContextService(IdentityDbContext context, ISessionService? sessionService = null)
    {
        _context = context;
        _sessionService = sessionService;
    }

    public int? GetCurrentUserId()
    {
        return _sessionService?.GetCurrentUserId();
    }

    public int? GetCurrentPositionId()
    {
        return _sessionService?.GetCurrentPositionId();
    }

    public async Task<int?> GetCurrentUserOrganizationIdAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return null;

        var user = await _context.Users
            .Where(u => u.Id == userId.Value && !u.IsDeleted)
            .Select(u => new { u.OrganizationId })
            .FirstOrDefaultAsync();

        return user?.OrganizationId;
    }

    public async Task<int?> GetCurrentUserDepartmentIdAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return null;

        var user = await _context.Users
            .Where(u => u.Id == userId.Value && !u.IsDeleted)
            .Select(u => new { u.DepartmentId })
            .FirstOrDefaultAsync();

        return user?.DepartmentId;
    }
}




