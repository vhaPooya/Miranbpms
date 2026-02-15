using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.Interfaces;
using Automation.Web.Extensions;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت مجوزها
/// </summary>
public class PermissionController : Controller
{
    private readonly IdentityDbContext _context;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(
        IdentityDbContext context,
        IPermissionService permissionService,
        ILogger<PermissionController> logger)
    {
        _context = context;
        _permissionService = permissionService;
        _logger = logger;
    }

    /// <summary>
    /// صفحه اصلی مدیریت مجوزها
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// دریافت ساختار درختی مجوزها
    /// </summary>
    [HttpGet]
    [Route("api/permissions/tree")]
    public async Task<IActionResult> GetPermissionTree()
    {
        try
        {
            var groups = await _context.PermissionGroups
                .Include(pg => pg.Permissions)
                .Where(pg => pg.ParentGroupId == null && !pg.IsDeleted)
                .OrderBy(pg => pg.DisplayOrder)
                .ToListAsync();

            var tree = groups.Select(g => BuildPermissionGroupNode(g)).ToList();

            return Json(new { success = true, data = tree });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permission tree");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت مجوزهای یک نقش
    /// </summary>
    [HttpGet]
    [Route("api/permissions/role/{roleId}")]
    public async Task<IActionResult> GetRolePermissions(int roleId)
    {
        try
        {
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            return Json(new { success = true, data = permissions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role permissions for role {RoleId}", roleId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت مجوزهای یک گروه
    /// </summary>
    [HttpGet]
    [Route("api/permissions/group/{groupId}")]
    public async Task<IActionResult> GetGroupPermissions(int groupId)
    {
        try
        {
            var permissions = await _permissionService.GetGroupPermissionsAsync(groupId);
            return Json(new { success = true, data = permissions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group permissions for group {GroupId}", groupId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت مجوزهای کاربر فعلی
    /// </summary>
    [HttpGet]
    [Route("api/permissions/user-permissions")]
    public async Task<IActionResult> GetUserPermissions()
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Json(new { success = true, permissions = permissions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// بررسی مجوز
    /// </summary>
    [HttpGet]
    [Route("api/permissions/check")]
    public async Task<IActionResult> CheckPermission([FromQuery] string code)
    {
        try
        {
            var userId = this.GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { success = false, error = "User not authenticated" });
            
            var hasPermission = await _permissionService.HasPermissionAsync(userId, code);
            return Json(new { success = true, hasPermission = hasPermission });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Code}", code);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// اختصاص مجوزها به نقش
    /// </summary>
    [HttpPost]
    [Route("api/permissions/role/{roleId}/assign")]
    public async Task<IActionResult> AssignPermissionsToRole(int roleId, [FromBody] AssignPermissionsDto dto)
    {
        try
        {
            // حذف مجوزهای قبلی
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
            
            _context.RolePermissions.RemoveRange(existingPermissions);

            // اضافه کردن مجوزهای جدید
            var permissions = await _context.Permissions
                .Where(p => dto.PermissionCodes.Contains(p.PermissionCode) && !p.IsDeleted)
                .ToListAsync();

            var rolePermissions = permissions.Select(p => new RolePermission
            {
                RoleId = roleId,
                PermissionId = p.Id,
                IsGranted = true
            }).ToList();

            await _context.RolePermissions.AddRangeAsync(rolePermissions);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to role {RoleId}", roleId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// اختصاص مجوزها به گروه
    /// </summary>
    [HttpPost]
    [Route("api/permissions/group/{groupId}/assign")]
    public async Task<IActionResult> AssignPermissionsToGroup(int groupId, [FromBody] AssignPermissionsDto dto)
    {
        try
        {
            // حذف مجوزهای قبلی
            var existingPermissions = await _context.GroupPermissions
                .Where(gp => gp.GroupId == groupId)
                .ToListAsync();
            
            _context.GroupPermissions.RemoveRange(existingPermissions);

            // اضافه کردن مجوزهای جدید
            var permissions = await _context.Permissions
                .Where(p => dto.PermissionCodes.Contains(p.PermissionCode) && !p.IsDeleted)
                .ToListAsync();

            var groupPermissions = permissions.Select(p => new GroupPermission
            {
                GroupId = groupId,
                PermissionId = p.Id,
                IsGranted = true
            }).ToList();

            await _context.GroupPermissions.AddRangeAsync(groupPermissions);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to group {GroupId}", groupId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    // Helper Methods
    private object BuildPermissionGroupNode(PermissionGroup group)
    {
        var children = _context.PermissionGroups
            .Include(pg => pg.Permissions)
            .Where(pg => pg.ParentGroupId == group.Id && !pg.IsDeleted)
            .OrderBy(pg => pg.DisplayOrder)
            .ToList()
            .Select(g => BuildPermissionGroupNode(g))
            .ToList();

        var permissions = group.Permissions
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                id = p.Id.ToString(),
                code = p.PermissionCode,
                name = p.PermissionName,
                description = p.Description,
                category = p.Category
            })
            .ToList();

        return new
        {
            id = group.Id.ToString(),
            code = group.GroupCode,
            name = group.GroupName,
            icon = group.Icon,
            description = group.Description,
            children = children,
            permissions = permissions
        };
    }
}





