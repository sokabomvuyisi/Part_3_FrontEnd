using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace RaceDay.Mvc.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role == "Organiser") return RedirectToAction("Dashboard", "Organiser");
        if (role == "Participant") return RedirectToAction("Index", "Participant");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
