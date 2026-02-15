using Microsoft.AspNetCore.Mvc;

namespace Automation.Module.Workflow.Controllers;

/// <summary>
/// API کنترلر ماژول فرآیند — با AddApplicationPart این اسمبلی به pipeline اضافه شده است.
/// </summary>
[ApiController]
[Route("api/module/workflow")]
public class WorkflowApiController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { module = "Workflow", status = "loaded" });
}
