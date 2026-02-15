using Microsoft.AspNetCore.Mvc;

namespace Automation.Module.FormBuilder.Controllers;

/// <summary>
/// API کنترلر ماژول فرم‌ساز — کنترلرهای اصلی (FormBuilderController, FormDataController) در Automation.Web قرار دارند؛
/// با AddApplicationPart این اسمبلی به pipeline اضافه شده‌اند در صورت انتقال به این ماژول.
/// </summary>
[ApiController]
[Route("api/module/formbuilder")]
public class FormBuilderApiController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { module = "FormBuilder", status = "loaded" });
}
