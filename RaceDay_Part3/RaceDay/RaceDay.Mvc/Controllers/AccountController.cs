using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api) => _api = api;

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, data, error) = await _api.RegisterAsync(model);
        if (success)
        {
            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }
        ModelState.AddModelError("", error ?? "Registration failed.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var (success, data, error) = await _api.LoginAsync(model);
        if (success && data != null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return data.Role == "Organiser"
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Participant");
        }
        ModelState.AddModelError("", error ?? "Invalid email or password.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }
}
