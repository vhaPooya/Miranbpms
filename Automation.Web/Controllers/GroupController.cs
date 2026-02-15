using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Automation.Web.Controllers;

/// <summary>
/// کنترلر مدیریت گروه‌ها
/// </summary>
public class GroupController : Controller
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<GroupController> _logger;

    public GroupController(IdentityDbContext context, ILogger<GroupController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// صفحه اصلی مدیریت گروه‌ها
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// دریافت لیست گروه‌ها
    /// </summary>
    [HttpGet]
    [Route("api/groups")]
    public async Task<IActionResult> GetGroups([FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Groups
                .Include(g => g.Members)
                .Where(g => !g.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(g => g.GroupName.Contains(search) || g.GroupCode.Contains(search));
            }

            var total = await query.CountAsync();
            var groups = await query
                .OrderBy(g => g.GroupName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new
                {
                    id = g.Id,
                    code = g.GroupCode,
                    name = g.GroupName,
                    description = g.Description,
                    memberCount = g.Members.Count(m => !m.IsDeleted),
                    isActive = g.IsActive,
                    creationDate = g.CreationDate
                })
                .ToListAsync();

            return Json(new { success = true, data = groups, total = total, page = page, pageSize = pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// دریافت جزئیات یک گروه
    /// </summary>
    [HttpGet]
    [Route("api/groups/{id}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        try
        {
            var group = await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .Include(g => g.GroupPermissions)
                    .ThenInclude(gp => gp.Permission)
                .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

            if (group == null)
                return NotFound();

            var result = new
            {
                id = group.Id,
                code = group.GroupCode,
                name = group.GroupName,
                description = group.Description,
                members = group.Members
                    .Where(m => !m.IsDeleted)
                    .Select(m => new
                    {
                        id = m.UserId,
                        username = m.User.Username,
                        fullName = m.User.FullName,
                        joinedDate = m.JoinedDate
                    }),
                permissions = group.GroupPermissions
                    .Where(gp => !gp.IsDeleted)
                    .Select(gp => new
                    {
                        id = gp.PermissionId,
                        code = gp.Permission.PermissionCode,
                        name = gp.Permission.PermissionName
                    }),
                isActive = group.IsActive
            };

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ایجاد گروه جدید
    /// </summary>
    [HttpPost]
    [Route("api/groups")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.Name))
            {
                return Json(new { success = false, error = "کد و نام گروه الزامی است" });
            }

            // بررسی تکراری نبودن کد
            var exists = await _context.Groups
                .AnyAsync(g => g.GroupCode == dto.Code && !g.IsDeleted);

            if (exists)
            {
                return Json(new { success = false, error = "کد گروه تکراری است" });
            }

            var group = new Group
            {
                GroupCode = dto.Code,
                GroupName = dto.Name,
                Description = dto.Description,
                IsActive = true
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = group.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// ویرایش گروه
    /// </summary>
    [HttpPut]
    [Route("api/groups/{id}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupDto dto)
    {
        try
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null || group.IsDeleted)
                return NotFound();

            group.GroupName = dto.Name ?? group.GroupName;
            group.Description = dto.Description ?? group.Description;
            group.IsActive = dto.IsActive ?? group.IsActive;
            group.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// حذف گروه (Soft Delete)
    /// </summary>
    [HttpDelete]
    [Route("api/groups/{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        try
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null || group.IsDeleted)
                return NotFound();

            group.IsDeleted = true;
            group.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// اضافه کردن عضو به گروه
    /// </summary>
    [HttpPost]
    [Route("api/groups/{groupId}/members")]
    public async Task<IActionResult> AddMember(int groupId, [FromBody] AddMemberDto dto)
    {
        try
        {
            var exists = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == dto.UserId && !gm.IsDeleted);

            if (exists)
            {
                return Json(new { success = false, error = "کاربر قبلاً در این گروه عضو است" });
            }

            var member = new GroupMember
            {
                GroupId = groupId,
                UserId = dto.UserId,
                Notes = dto.Notes,
                JoinedDate = DateTime.UtcNow
            };

            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = member.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to group {GroupId}", groupId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// حذف عضو از گروه
    /// </summary>
    [HttpDelete]
    [Route("api/groups/{groupId}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int groupId, int userId)
    {
        try
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId && !gm.IsDeleted);

            if (member == null)
                return NotFound();

            member.IsDeleted = true;
            member.EditDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from group {GroupId}", groupId);
            return Json(new { success = false, error = ex.Message });
        }
    }
}





