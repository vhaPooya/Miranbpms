using Automation.Core.Constants;
using Automation.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Automation.Web.Filters;

/// <summary>
/// قرار دادن آیتم‌های زمینه جاری (OUserId, OPosId, OFEIC, OFEC و ...) در ViewBag برای استفاده در Viewها و اسکریپت‌ها.
/// </summary>
public class CurrentContextViewBagFilter : IAsyncActionFilter
{
    private readonly ICurrentContext _currentContext;

    public CurrentContextViewBagFilter(ICurrentContext currentContext)
    {
        _currentContext = currentContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        if (result.Result is ViewResult && context.Controller is Controller controller)
        {
            controller.ViewBag.OUserId = _currentContext.UserId;
            controller.ViewBag.OPosId = _currentContext.PositionId;
            controller.ViewBag.OFEIC = _currentContext.FormId ?? (object?)_currentContext.FormCode;
            controller.ViewBag.OFEC = _currentContext.RecordId;
            controller.ViewBag.OOrganizationId = _currentContext.OrganizationId;
            controller.ViewBag.ODepartmentId = _currentContext.DepartmentId;
            controller.ViewBag.OSecretariatId = _currentContext.SecretariatId;
            controller.ViewBag.OWorkflowInstanceId = _currentContext.WorkflowInstanceId;
            controller.ViewBag.CurrentContext = _currentContext;
            controller.ViewBag.CurrentContextNames = typeof(CurrentContextNames);
        }
    }
}
