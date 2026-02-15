using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Automation.Infrastructure.Services;

/// <summary>
/// تنظیم زمینه جاری (OFEIC, OFEC, OWorkflowInstanceId) برای درخواست جاری.
/// </summary>
public class CurrentContextSetter : ICurrentContextSetter
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private const string HttpItemFormId = "CurrentContext_FormId";
    private const string HttpItemRecordId = "CurrentContext_RecordId";
    private const string HttpItemFormCode = "CurrentContext_FormCode";
    private const string HttpItemWorkflowInstanceId = "CurrentContext_WorkflowInstanceId";

    public CurrentContextSetter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetFormContext(int? formId, long? recordId = null)
    {
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items == null) return;

        if (formId.HasValue)
            items[HttpItemFormId] = formId.Value;
        else
            items.Remove(HttpItemFormId);

        if (recordId.HasValue)
            items[HttpItemRecordId] = recordId.Value;
        else
            items.Remove(HttpItemRecordId);
    }

    public void SetFormCode(string? formCode)
    {
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items == null) return;

        if (!string.IsNullOrEmpty(formCode))
            items[HttpItemFormCode] = formCode;
        else
            items.Remove(HttpItemFormCode);
    }

    public void SetWorkflowInstanceId(int? workflowInstanceId)
    {
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items == null) return;

        if (workflowInstanceId.HasValue)
            items[HttpItemWorkflowInstanceId] = workflowInstanceId.Value;
        else
            items.Remove(HttpItemWorkflowInstanceId);
    }

    public void ClearRequestContext()
    {
        var items = _httpContextAccessor.HttpContext?.Items;
        if (items == null) return;

        items.Remove(HttpItemFormId);
        items.Remove(HttpItemRecordId);
        items.Remove(HttpItemFormCode);
        items.Remove(HttpItemWorkflowInstanceId);
    }
}
