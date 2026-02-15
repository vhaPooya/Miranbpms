using Automation.Core.Constants;
using Automation.Core.Interfaces;
using Automation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Automation.Infrastructure.Services;

/// <summary>
/// زمینه جاری درخواست — OUserId, OPosId, OFEIC, OFEC و سایر آیتم‌ها در سراسر پروژه با نام‌های ثابت در دسترس.
/// </summary>
public class CurrentContext : ICurrentContext
{
    private readonly ISessionService _sessionService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IdentityDbContext _identityContext;
    private readonly AutomationDbContext _automationContext;

    private const string HttpItemFormId = "CurrentContext_FormId";
    private const string HttpItemRecordId = "CurrentContext_RecordId";
    private const string HttpItemFormCode = "CurrentContext_FormCode";
    private const string HttpItemWorkflowInstanceId = "CurrentContext_WorkflowInstanceId";

    public CurrentContext(
        ISessionService sessionService,
        IHttpContextAccessor httpContextAccessor,
        IdentityDbContext identityContext,
        AutomationDbContext automationContext)
    {
        _sessionService = sessionService;
        _httpContextAccessor = httpContextAccessor;
        _identityContext = identityContext;
        _automationContext = automationContext;
    }

    public int? UserId => _sessionService.GetCurrentUserId();
    public int? PositionId => _sessionService.GetCurrentPositionId();
    public int? SecretariatId => _sessionService.GetCurrentSecretariatId();

    public int? FormId
    {
        get
        {
            var v = _httpContextAccessor.HttpContext?.Items[HttpItemFormId];
            if (v is int i) return i;
            if (v is string s && int.TryParse(s, out var id)) return id;
            return null;
        }
    }

    public long? RecordId
    {
        get
        {
            var v = _httpContextAccessor.HttpContext?.Items[HttpItemRecordId];
            if (v is long l) return l;
            if (v is int i) return i;
            if (v is string s && long.TryParse(s, out var id)) return id;
            return null;
        }
    }

    public string? FormCode
    {
        get
        {
            var fromItems = _httpContextAccessor.HttpContext?.Items[HttpItemFormCode];
            if (fromItems is string code) return code;
            if (FormId.HasValue)
            {
                var form = _automationContext.Forms.AsNoTracking()
                    .Where(f => f.Id == FormId.Value && !f.IsDeleted)
                    .Select(f => f.FormCode)
                    .FirstOrDefault();
                return form;
            }
            return null;
        }
    }

    public int? OrganizationId => _identityContext.Users
        .AsNoTracking()
        .Where(u => u.Id == UserId && !u.IsDeleted)
        .Select(u => (int?)u.OrganizationId)
        .FirstOrDefault();

    public int? DepartmentId => _identityContext.Users
        .AsNoTracking()
        .Where(u => u.Id == UserId && !u.IsDeleted)
        .Select(u => (int?)u.DepartmentId)
        .FirstOrDefault();

    public int? WorkflowInstanceId
    {
        get
        {
            var v = _httpContextAccessor.HttpContext?.Items[HttpItemWorkflowInstanceId];
            if (v is int i) return i;
            if (v is string s && int.TryParse(s, out var id)) return id;
            return null;
        }
    }

    public object? GetValue(string contextKey)
    {
        if (string.IsNullOrEmpty(contextKey)) return null;
        return contextKey switch
        {
            CurrentContextNames.OUserId => UserId,
            CurrentContextNames.OPosId => PositionId,
            CurrentContextNames.OFEIC => FormId ?? (object?)FormCode,
            CurrentContextNames.OFEC => RecordId,
            CurrentContextNames.OOrganizationId => OrganizationId,
            CurrentContextNames.ODepartmentId => DepartmentId,
            CurrentContextNames.OSecretariatId => SecretariatId,
            CurrentContextNames.OWorkflowInstanceId => WorkflowInstanceId,
            _ => null
        };
    }
}
