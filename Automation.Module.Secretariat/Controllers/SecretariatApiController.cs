using Microsoft.AspNetCore.Mvc;

namespace Automation.Module.Secretariat.Controllers;

/// <summary>
/// API کنترلر ماژول دبیرخانه — با AddApplicationPart این اسمبلی به pipeline اضافه شده است.
/// </summary>
[ApiController]
[Route("api/module/secretariat")]
public class SecretariatApiController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { module = "Secretariat", status = "loaded" });
}
