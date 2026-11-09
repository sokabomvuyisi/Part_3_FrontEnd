using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : RaceDayControllerBase
{
    private readonly ApiService _api;

    public ParticipantController(ApiService api) => _api = api;

    private IActionResult? RequireParticipant()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Index", "Home");
        return null;
    }

    // Home - upcoming events with optional filter
    public async Task<IActionResult> Index(string? type = null, string? location = null)
    {
        var auth = RequireParticipant(); if (auth != null) return auth;
        var events = OrEmpty(await _api.GetEventsAsync(), "the events");
        events = events.Where(e => e.EventDate >= DateTime.Today).OrderBy(e => e.EventDate).ToList();
        if (!string.IsNullOrWhiteSpace(type))
            events = events.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(location))
            events = events.Where(e => e.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
        ViewBag.Type = type;
        ViewBag.Location = location;
        return View(events);
    }

    // Event detail + enrol
    public async Task<IActionResult> EventDetail(int id)
    {
        var auth = RequireParticipant(); if (auth != null) return auth;
        var (ok, ev, error) = await _api.GetEventAsync(id);
        if (!ok || ev == null)
        {
            TempData["Error"] = "Could not load the event. " + error;
            return RedirectToAction("Index");
        }
        ViewBag.Categories = OrEmpty(await _api.GetCategoriesAsync(id), "the categories");
        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var auth = RequireParticipant(); if (auth != null) return auth;

        if (categoryId < 1)
        {
            TempData["Error"] = "Please select a category before enrolling.";
            return RedirectToAction("EventDetail", new { id = eventId });
        }

        var (success, _, error) = await _api.EnrolAsync(new CreateEnrolmentRequest
        {
            EventId = eventId,
            CategoryId = categoryId
        });
        if (success)
            TempData["Success"] = "You have successfully enrolled!";
        else
            TempData["Error"] = error ?? "Enrolment failed.";
        return RedirectToAction("EventDetail", new { id = eventId });
    }

    // My Events
    public async Task<IActionResult> MyEvents()
    {
        var auth = RequireParticipant(); if (auth != null) return auth;
        var list = OrEmpty(await _api.GetMyEnrolmentsAsync(), "your enrolments");
        return View(list);
    }

    // My Results
    public async Task<IActionResult> MyResults()
    {
        var auth = RequireParticipant(); if (auth != null) return auth;
        var list = OrEmpty(await _api.GetMyResultsAsync(), "your results");
        return View(list);
    }

    // Profile
    public async Task<IActionResult> Profile()
    {
        var auth = RequireParticipant(); if (auth != null) return auth;
        var (ok, profile, error) = await _api.GetProfileAsync();
        if (!ok || profile == null)
        {
            TempData["Error"] = "Could not load your profile. " + error;
            // If the API session expired the MVC session was cleared too -> back to login.
            return HttpContext.Session.GetInt32("UserId") == null
                ? RedirectToAction("Login", "Account")
                : RedirectToAction("Index");
        }
        return View(ToViewModel(profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Form")] UpdateProfileRequest model)
    {
        var auth = RequireParticipant(); if (auth != null) return auth;

        if (!ModelState.IsValid)
        {
            var (ok, profile, error) = await _api.GetProfileAsync();
            var vm = new ProfilePageViewModel { Profile = profile ?? new ProfileDto(), Form = model };
            if (!ok) TempData["Error"] = "Could not load your profile. " + error;
            return View("Profile", vm);
        }

        var (success, _, updateError) = await _api.UpdateProfileAsync(model);
        if (success) TempData["Success"] = "Profile updated.";
        else TempData["Error"] = "Your profile could not be updated. " + updateError;
        return RedirectToAction("Profile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        var auth = RequireParticipant(); if (auth != null) return auth;
        if (picture == null || picture.Length == 0)
        {
            TempData["Error"] = "Please choose an image to upload.";
            return RedirectToAction("Profile");
        }

        var (success, _, error) = await _api.UploadProfilePictureAsync(picture);
        if (success) TempData["Success"] = "Profile picture updated.";
        else TempData["Error"] = "The picture could not be uploaded. " + error;
        return RedirectToAction("Profile");
    }

    private static ProfilePageViewModel ToViewModel(ProfileDto profile) => new()
    {
        Profile = profile,
        Form = new UpdateProfileRequest
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Email = profile.Email
        }
    };
}
