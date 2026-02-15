using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automation.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس مدیریت مجوزها
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IdentityDbContext _identityContext;
    private readonly AutomationDbContext _context;
    private readonly CoreDbContext _coreContext;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IdentityDbContext identityContext, AutomationDbContext context, CoreDbContext coreContext, ILogger<PermissionService> logger)
    {
        _identityContext = identityContext;
        _context = context;
        _coreContext = coreContext;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode)
    {
        try
        {
            // بررسی مجوز از طریق نقش‌ها
            var userRoles = await _identityContext.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var hasRolePermission = await _identityContext.RolePermissions
                .Include(rp => rp.Permission)
                .AnyAsync(rp => userRoles.Contains(rp.RoleId) && 
                               rp.Permission.PermissionCode == permissionCode && 
                               rp.IsGranted && 
                               !rp.IsDeleted);

            if (hasRolePermission)
                return true;

            // بررسی مجوز از طریق گروه‌ها
            var userGroups = await _identityContext.GroupMembers
                .Where(gm => gm.UserId == userId && !gm.IsDeleted)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var hasGroupPermission = await _identityContext.GroupPermissions
                .Include(gp => gp.Permission)
                .AnyAsync(gp => userGroups.Contains(gp.GroupId) && 
                               gp.Permission.PermissionCode == permissionCode && 
                               gp.IsGranted && 
                               !gp.IsDeleted);

            return hasGroupPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionCode} for user {UserId}", permissionCode, userId);
            return false;
        }
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, int? resourceId)
    {
        // در حال حاضر همان HasPermissionAsync است
        // می‌تواند در آینده برای مجوزهای سطح Resource توسعه یابد
        return await HasPermissionAsync(userId, permissionCode);
    }

    public async Task<bool> HasFormPermissionAsync(int userId, int formId, string permissionCode)
    {
        // بررسی مجوز عمومی
        var hasGeneralPermission = await HasPermissionAsync(userId, permissionCode);
        if (!hasGeneralPermission)
            return false;

        // TODO: بررسی مجوزهای سطح فرم (FormPermissions table)
        // در حال حاضر فقط مجوز عمومی بررسی می‌شود
        
        return true;
    }

    public async Task<bool> HasFieldPermissionAsync(int userId, int formId, string fieldName, string permissionCode)
    {
        // بررسی مجوز فرم
        var hasFormPermission = await HasFormPermissionAsync(userId, formId, permissionCode);
        if (!hasFormPermission)
            return false;

        // TODO: بررسی مجوزهای سطح فیلد (FormPermissions with FieldName)
        // در حال حاضر فقط مجوز فرم بررسی می‌شود
        
        return true;
    }

    public async Task<bool> CanAccessDocumentAsync(int userId, int documentId)
    {
        try
        {
            // TODO: پیاده‌سازی منطق دسترسی به مدرک در گردش
            // در حال حاضر فقط بررسی مجوز عمومی
            return await HasPermissionAsync(userId, "FORM_DATA_VIEW");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking document access for user {UserId} and document {DocumentId}", userId, documentId);
            return false;
        }
    }

    public async Task<bool> CanEditDocumentAsync(int userId, int documentId)
    {
        try
        {
            // بررسی آیا مدرک امضاء شده است؟
            // TODO: بررسی از جدول FormData یا Document
            // var document = await _context.Documents.FindAsync(documentId);
            // if (document?.IsSigned == true)
            // {
            //     return await HasPermissionAsync(userId, "FORM_SIGNED_EDIT");
            // }

            // بررسی Action Type در ارجاع فعلی
            // دریافت ارجاع‌های فعال
            var activeReferrals = await _context.DocumentReferrals
                .Include(dr => dr.ActionType)
                .Where(dr => dr.DocumentId == documentId && dr.Status == "PENDING")
                .ToListAsync();

            // بررسی آیا کاربر در لیست گیرندگان است؟
            DocumentReferral? currentReferral = null;
            foreach (var referral in activeReferrals)
            {
                if (referral.ReferredToUserId == userId)
                {
                    currentReferral = referral;
                    break;
                }
                
                if (referral.ReferredToRoleId.HasValue && await UserHasRoleAsync(userId, referral.ReferredToRoleId.Value))
                {
                    currentReferral = referral;
                    break;
                }
                
                if (referral.ReferredToGroupId.HasValue && await UserInGroupAsync(userId, referral.ReferredToGroupId.Value))
                {
                    currentReferral = referral;
                    break;
                }
            }

            if (currentReferral == null)
            {
                currentReferral = activeReferrals.OrderByDescending(dr => dr.ReferredDate).FirstOrDefault();
            }

            if (currentReferral?.ActionType != null)
            {
                // اگر Action Type قفل باشد، مدرک قابل ویرایش نیست
                return currentReferral.ActionType.IsEditable;
            }

            // بررسی مجوزهای عادی
            return await HasPermissionAsync(userId, "FORM_DATA_EDIT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permission for user {UserId} and document {DocumentId}", userId, documentId);
            return false;
        }
    }

    public async Task<bool> IsDocumentLockedAsync(int documentId)
    {
        try
        {
            // TODO: بررسی از جدول FormData یا Document
            // var document = await _context.Documents.FindAsync(documentId);
            // return document?.IsSigned == true;
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking document lock status for document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        try
        {
            var permissions = new HashSet<string>();

            // مجوزهای از طریق نقش‌ها
            var userRoles = await _identityContext.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var rolePermissions = await _identityContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => userRoles.Contains(rp.RoleId) && rp.IsGranted && !rp.IsDeleted)
                .Select(rp => rp.Permission.PermissionCode)
                .ToListAsync();

            foreach (var perm in rolePermissions)
                permissions.Add(perm);

            // مجوزهای از طریق گروه‌ها
            var userGroups = await _identityContext.GroupMembers
                .Where(gm => gm.UserId == userId && !gm.IsDeleted)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var groupPermissions = await _identityContext.GroupPermissions
                .Include(gp => gp.Permission)
                .Where(gp => userGroups.Contains(gp.GroupId) && gp.IsGranted && !gp.IsDeleted)
                .Select(gp => gp.Permission.PermissionCode)
                .ToListAsync();

            foreach (var perm in groupPermissions)
                permissions.Add(perm);

            return permissions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetRolePermissionsAsync(int roleId)
    {
        try
        {
            return await _identityContext.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && rp.IsGranted && !rp.IsDeleted)
                .Select(rp => rp.Permission.PermissionCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role permissions for role {RoleId}", roleId);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetGroupPermissionsAsync(int groupId)
    {
        try
        {
            return await _identityContext.GroupPermissions
                .Include(gp => gp.Permission)
                .Where(gp => gp.GroupId == groupId && gp.IsGranted && !gp.IsDeleted)
                .Select(gp => gp.Permission.PermissionCode)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group permissions for group {GroupId}", groupId);
            return new List<string>();
        }
    }

    public async Task<ActionType?> GetActionTypeByCodeAsync(string actionCode)
    {
        try
        {
            return await _coreContext.ActionTypes
                .FirstOrDefaultAsync(at => at.ActionCode == actionCode && !at.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action type by code {ActionCode}", actionCode);
            return null;
        }
    }

    public async Task CreateFormPermissionsAsync(int formId, string formName)
    {
        try
        {
            // دریافت فرم
            var form = await _context.Forms.FindAsync(formId);
            if (form == null)
                return;

            var groupCode = $"FRMB_{form.FormCode}";

            // بررسی وجود قبلی
            var existingGroup = await _identityContext.PermissionGroups
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(pg => pg.GroupCode == groupCode && !pg.IsDeleted);
            if (existingGroup != null)
                return;

            // ایجاد دسته مجوز برای فرم
            var formPermissionGroup = new PermissionGroup
            {
                GroupCode = groupCode,
                GroupName = formName,
                Icon = "bi-file-earmark-text",
                DisplayOrder = 100,
                Description = $"مجوزهای مربوط به فرم {formName}"
            };

            _identityContext.PermissionGroups.Add(formPermissionGroup);
            await _identityContext.SaveChangesAsync();

            // ایجاد مجوزهای کامل فرمساز
            var prefix = $"FRMB_{form.FormCode}";
            var formPermissions = new List<Permission>
            {
                new() { PermissionCode = $"{prefix}_DATA_ENTRY", PermissionName = $"مجوز ورود اطلاعات برای اسناد عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 1 },
                new() { PermissionCode = $"{prefix}_VIEW", PermissionName = $"مجوز مشاهده اسناد عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 2 },
                new() { PermissionCode = $"{prefix}_EDIT_DATA", PermissionName = $"مجوز ویرایش اطلاعات فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 3 },
                new() { PermissionCode = $"{prefix}_EDIT_LOCKED", PermissionName = $"مجوز ویرایش اطلاعات قفل شده در تمام اسناد عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 4 },
                new() { PermissionCode = $"{prefix}_DELETE_DATA", PermissionName = $"مجوز حذف اطلاعات اسناد عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 5 },
                new() { PermissionCode = $"{prefix}_DELETE", PermissionName = $"مجوز حذف فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 6 },
                new() { PermissionCode = $"{prefix}_DESIGN", PermissionName = $"مجوز طراحی فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 7 },
                new() { PermissionCode = $"{prefix}_EDIT_STRUCT", PermissionName = $"مجوز ویرایش ساختار فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 8 },
                new() { PermissionCode = $"{prefix}_PRINT", PermissionName = $"مجوز چاپ محلی فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 9 },
                new() { PermissionCode = $"{prefix}_SEARCH_PUBLIC", PermissionName = $"مجوز جستجو در اسناد غیرخصوصی عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 10 },
                new() { PermissionCode = $"{prefix}_SEARCH_ALL", PermissionName = $"مجوز جستجوی کلیه اسناد عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 11 },
                new() { PermissionCode = $"{prefix}_PRIVATE_MANAGE", PermissionName = $"مجوز مدیریت اسناد خصوصی عادی از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 12 },
                new() { PermissionCode = $"{prefix}_VIEW_FLOW", PermissionName = $"مجوز مشاهده گردش اسناد از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 13 },
                new() { PermissionCode = $"{prefix}_VIEW_HIDDEN_FLOW", PermissionName = $"مجوز مشاهده گردش مخفی اسناد از نوع فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 14 },
                new() { PermissionCode = $"{prefix}_VIEW_CHAIN", PermissionName = $"مجوز مشاهده زنجیره مدارک مرتبط با مدرک فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 15 },
                new() { PermissionCode = $"{prefix}_ADD_FOLLOW", PermissionName = $"مجوز افزودن پیرو به فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 16 },
                new() { PermissionCode = $"{prefix}_DEL_FOLLOW", PermissionName = $"مجوز حذف پیرو از فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 17 },
                new() { PermissionCode = $"{prefix}_ADD_ATTACH", PermissionName = $"مجوز افزودن پیوست به فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 18 },
                new() { PermissionCode = $"{prefix}_DEL_ATTACH", PermissionName = $"مجوز حذف پیوست از فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 19 },
                new() { PermissionCode = $"{prefix}_ADD_REF", PermissionName = $"مجوز افزودن عطف به فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 20 },
                new() { PermissionCode = $"{prefix}_DEL_REF", PermissionName = $"مجوز حذف عطف از فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 21 },
                new() { PermissionCode = $"{prefix}_ADD_RELATED", PermissionName = $"مجوز افزودن مدارک در ارتباط با فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 22 },
                new() { PermissionCode = $"{prefix}_DEL_RELATED", PermissionName = $"مجوز حذف مدارک در ارتباط با فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 23 },
                new() { PermissionCode = $"{prefix}_ADD_PARAPH", PermissionName = $"مجوز افزودن پاراف به فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 24 },
                new() { PermissionCode = $"{prefix}_ADD_HIDDEN_PARAPH", PermissionName = $"مجوز افزودن پاراف مخفی به فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 25 },
                new() { PermissionCode = $"{prefix}_VIEW_PARAPH", PermissionName = $"مجوز مشاهده لیست پاراف‌های مدرک فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 26 },
                new() { PermissionCode = $"{prefix}_VIEW_HIDDEN_PARAPH", PermissionName = $"مجوز مشاهده لیست پاراف‌های مخفی مدرک فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 27 },
                new() { PermissionCode = $"{prefix}_FAX", PermissionName = $"مجوز ارسال فاکس مدرک فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 28 },
                new() { PermissionCode = $"{prefix}_ADV_SEARCH", PermissionName = $"مجوز تنظیمات جستجوی پیشرفته برای فرم {formName}", Category = "formbuilder", PermissionGroupId = formPermissionGroup.Id, DisplayOrder = 29 },
            };

            await _identityContext.Permissions.AddRangeAsync(formPermissions);
            await _identityContext.SaveChangesAsync();

            _logger.LogInformation("Form permissions created for form {FormId} ({FormName}) - {Count} permissions", formId, formName, formPermissions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating form permissions for form {FormId}", formId);
            throw;
        }
    }

    // Helper Methods
    private async Task<bool> UserHasRoleAsync(int userId, int roleId)
    {
        return await _identityContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted);
    }

    private async Task<bool> UserInGroupAsync(int userId, int groupId)
    {
        return await _identityContext.GroupMembers
            .AnyAsync(gm => gm.UserId == userId && gm.GroupId == groupId && !gm.IsDeleted);
    }

    public async Task CreateReportPermissionsAsync(int reportId, string reportName)
    {
        try
        {
            // دریافت دسته مجوز report
            var reportGroup = await _identityContext.PermissionGroups
                .FirstOrDefaultAsync(pg => pg.GroupCode == "REPORT");

            if (reportGroup == null)
            {
                _logger.LogWarning("Report permission group not found");
                return;
            }

            // ایجاد دسته مجوز برای این report خاص
            var reportPermissionGroup = new PermissionGroup
            {
                GroupCode = $"REPORT_{reportId}",
                GroupName = reportName,
                Icon = "bi-graph-up",
                ParentGroupId = reportGroup.Id,
                DisplayOrder = 1000,
                Description = $"مجوزهای مربوط به گزارش {reportName}"
            };

            _identityContext.PermissionGroups.Add(reportPermissionGroup);
            await _identityContext.SaveChangesAsync();

            // ایجاد مجوزهای report
            var reportPermissions = new List<Permission>
            {
                new() { PermissionCode = $"REPORT_{reportId}_VIEW", PermissionName = $"مشاهده {reportName}", Category = "REPORT", PermissionGroupId = reportPermissionGroup.Id, DisplayOrder = 1, Description = $"امکان مشاهده گزارش {reportName}" },
                new() { PermissionCode = $"REPORT_{reportId}_EXECUTE", PermissionName = $"اجرای {reportName}", Category = "REPORT", PermissionGroupId = reportPermissionGroup.Id, DisplayOrder = 2, Description = $"امکان اجرا و مشاهده نتایج گزارش {reportName}" },
                new() { PermissionCode = $"REPORT_{reportId}_EXPORT", PermissionName = $"خروجی {reportName}", Category = "REPORT", PermissionGroupId = reportPermissionGroup.Id, DisplayOrder = 3, Description = $"امکان خروجی گرفتن از گزارش {reportName}" }
            };

            await _identityContext.Permissions.AddRangeAsync(reportPermissions);
            await _identityContext.SaveChangesAsync();

            _logger.LogInformation("Report permissions created for report {ReportId} ({ReportName})", reportId, reportName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report permissions for report {ReportId}", reportId);
            throw;
        }
    }
}





