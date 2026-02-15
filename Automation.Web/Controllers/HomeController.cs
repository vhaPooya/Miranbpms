using Microsoft.AspNetCore.Mvc;

namespace Automation.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult MainPage()
    {
        return PartialView("_MainPage");
    }
}



