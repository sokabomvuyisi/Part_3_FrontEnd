using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Services;

/// <summary>
/// Thin wrapper around the RaceDay API. Every call is wrapped in try/catch and returns
/// (Success, Data, Error) so controllers can show a friendly message through TempData["Error"]
/// instead of letting an exception reach the generic error page.
/// </summary>
public class ApiService
{
    private const string ApiCookieKey = "ApiSessionCookie";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession? Session => _httpContextAccessor.HttpContext?.Session;

    // The handler has UseCookies = false, so the API session cookie belongs to the CURRENT user's
    // MVC session and is attached explicitly to every request.
    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");
        var apiCookie = Session?.GetString(ApiCookieKey);
        if (!string.IsNullOrEmpty(apiCookie))
        {
            client.DefaultRequestHeaders.Remove("Cookie");
            client.DefaultRequestHeaders.Add("Cookie", apiCookie);
        }
        return client;
    }

    private static StringContent Json(object body) =>
        new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    // ---------- core helpers ----------
    private async Task<(bool Success, T? Data, string? Error)> SendAsync<T>(
        Func<HttpClient, Task<HttpResponseMessage>> send,
        Action<HttpResponseMessage>? onSuccess = null)
    {
        try
        {
            using var response = await send(CreateClient());
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                onSuccess?.Invoke(response);
                T? data = string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
                return (true, data, null);
            }
            return (false, default, ErrorFor(response.StatusCode, json));
        }
        catch (Exception ex)
        {
            return (false, default, FriendlyError(ex));
        }
    }

    private async Task<(bool Success, string? Error)> SendAsync(Func<HttpClient, Task<HttpResponseMessage>> send)
    {
        try
        {
            using var response = await send(CreateClient());
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, ErrorFor(response.StatusCode, await response.Content.ReadAsStringAsync()));
        }
        catch (Exception ex)
        {
            return (false, FriendlyError(ex));
        }
    }

    private static MultipartFormDataContent FileContent(IFormFile file)
    {
        var content = new MultipartFormDataContent();
        var stream = new StreamContent(file.OpenReadStream());
        stream.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(stream, "file", file.FileName);
        return content;
    }

    // ========== AUTH ==========
    public Task<(bool Success, AuthResponse? Data, string? Error)> RegisterAsync(RegisterRequest request) =>
        SendAsync<AuthResponse>(c => c.PostAsync("api/auth/register", Json(request)));

    public async Task<(bool Success, AuthResponse? Data, string? Error)> LoginAsync(LoginRequest request)
    {
        // Start from a clean API session so a stale cookie from a previous login is never reused.
        Session?.Remove(ApiCookieKey);

        var result = await SendAsync<AuthResponse>(
            c => c.PostAsync("api/auth/login", Json(request)),
            response =>
            {
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    // Keep only "name=value" of each Set-Cookie header (drop path/secure/samesite...).
                    var pairs = cookies.Select(c => c.Split(';')[0].Trim()).Where(c => c.Length > 0);
                    Session?.SetString(ApiCookieKey, string.Join("; ", pairs));
                }
            });

        if (result.Success && result.Data != null && Session != null)
        {
            Session.SetInt32("UserId", result.Data.UserId);
            Session.SetString("Role", result.Data.Role);
            Session.SetString("FirstName", result.Data.FirstName);
            Session.SetString("LastName", result.Data.LastName);
            Session.SetString("Email", result.Data.Email);
        }
        else
        {
            Session?.Clear();
        }
        return result;
    }

    public async Task LogoutAsync()
    {
        await SendAsync(c => c.PostAsync("api/auth/logout", null));
        Session?.Clear();
    }

    // ========== EVENTS ==========
    public Task<(bool Success, List<EventDto>? Data, string? Error)> GetEventsAsync() =>
        SendAsync<List<EventDto>>(c => c.GetAsync("api/events"));

    public Task<(bool Success, List<EventDto>? Data, string? Error)> GetMyEventsAsync() =>
        SendAsync<List<EventDto>>(c => c.GetAsync("api/events/mine"));

    public Task<(bool Success, EventDto? Data, string? Error)> GetEventAsync(int id) =>
        SendAsync<EventDto>(c => c.GetAsync($"api/events/{id}"));

    public Task<(bool Success, EventDto? Data, string? Error)> CreateEventAsync(CreateEventRequest request) =>
        SendAsync<EventDto>(c => c.PostAsync("api/events", Json(request)));

    public Task<(bool Success, EventDto? Data, string? Error)> UpdateEventAsync(int id, UpdateEventRequest request) =>
        SendAsync<EventDto>(c => c.PutAsync($"api/events/{id}", Json(request)));

    public Task<(bool Success, string? Error)> DeleteEventAsync(int id) =>
        SendAsync(c => c.DeleteAsync($"api/events/{id}"));

    public async Task<(bool Success, string? Url, string? Error)> UploadEventBannerAsync(int eventId, IFormFile file)
    {
        var r = await SendAsync<BlobUploadResponse>(async c =>
        {
            using var content = FileContent(file);
            return await c.PostAsync($"api/events/{eventId}/banner", content);
        });
        return (r.Success, r.Data?.Url, r.Error);
    }

    // ========== CATEGORIES ==========
    public Task<(bool Success, List<CategoryDto>? Data, string? Error)> GetCategoriesAsync(int eventId) =>
        SendAsync<List<CategoryDto>>(c => c.GetAsync($"api/events/{eventId}/categories"));

    public Task<(bool Success, CategoryDto? Data, string? Error)> CreateCategoryAsync(int eventId, CreateCategoryRequest request) =>
        SendAsync<CategoryDto>(c => c.PostAsync($"api/events/{eventId}/categories", Json(request)));

    public Task<(bool Success, string? Error)> DeleteCategoryAsync(int id) =>
        SendAsync(c => c.DeleteAsync($"api/categories/{id}"));

    // ========== ENROLMENTS ==========
    public Task<(bool Success, EnrolmentDto? Data, string? Error)> EnrolAsync(CreateEnrolmentRequest request) =>
        SendAsync<EnrolmentDto>(c => c.PostAsync("api/enrolments", Json(request)));

    public Task<(bool Success, List<EnrolmentDto>? Data, string? Error)> GetMyEnrolmentsAsync() =>
        SendAsync<List<EnrolmentDto>>(c => c.GetAsync("api/enrolments/mine"));

    public Task<(bool Success, List<EnrolmentDto>? Data, string? Error)> GetEventEnrolmentsAsync(int eventId) =>
        SendAsync<List<EnrolmentDto>>(c => c.GetAsync($"api/enrolments/event/{eventId}"));

    public Task<(bool Success, string? Error)> UpdateEnrolmentStatusAsync(int id, string status) =>
        SendAsync(c => c.PutAsync($"api/enrolments/{id}/status", Json(new UpdateEnrolmentStatusRequest { Status = status })));

    // ========== RESULTS ==========
    public Task<(bool Success, ResultDto? Data, string? Error)> CreateResultAsync(CreateResultRequest request) =>
        SendAsync<ResultDto>(c => c.PostAsync("api/results", Json(request)));

    public Task<(bool Success, List<ResultDto>? Data, string? Error)> GetMyResultsAsync() =>
        SendAsync<List<ResultDto>>(c => c.GetAsync("api/results/mine"));

    public Task<(bool Success, List<ResultDto>? Data, string? Error)> GetEventResultsAsync(int eventId) =>
        SendAsync<List<ResultDto>>(c => c.GetAsync($"api/results/event/{eventId}"));

    // ========== PROFILE ==========
    public Task<(bool Success, ProfileDto? Data, string? Error)> GetProfileAsync() =>
        SendAsync<ProfileDto>(c => c.GetAsync("api/profile/me"));

    public Task<(bool Success, ProfileDto? Data, string? Error)> UpdateProfileAsync(UpdateProfileRequest request) =>
        SendAsync<ProfileDto>(c => c.PutAsync("api/profile/me", Json(request)));

    public async Task<(bool Success, string? Url, string? Error)> UploadProfilePictureAsync(IFormFile file)
    {
        var r = await SendAsync<BlobUploadResponse>(async c =>
        {
            using var content = FileContent(file);
            return await c.PostAsync("api/profile/me/picture", content);
        });
        return (r.Success, r.Data?.Url, r.Error);
    }

    // ========== ERROR HELPERS ==========
    private string ErrorFor(HttpStatusCode status, string json)
    {
        var message = ExtractMessage(json);

        if (status == HttpStatusCode.Unauthorized)
        {
            // The API session is gone (expired or never created): drop the MVC session too so the
            // user is sent back to the login page on their next click.
            Session?.Clear();
            return string.IsNullOrWhiteSpace(message)
                ? "Your session has expired. Please log in again."
                : message;
        }

        if (!string.IsNullOrWhiteSpace(message)) return message;

        return status switch
        {
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "The requested item could not be found.",
            HttpStatusCode.BadRequest => "The request was not valid. Please check your input.",
            _ => $"The API returned an unexpected error ({(int)status})."
        };
    }

    // Understands { "message": "..." } as well as ASP.NET validation problem details.
    private static string ExtractMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    return m.GetString() ?? string.Empty;

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = new List<string>();
                    foreach (var prop in errors.EnumerateObject())
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                            messages.AddRange(prop.Value.EnumerateArray()
                                .Where(v => v.ValueKind == JsonValueKind.String)
                                .Select(v => v.GetString() ?? string.Empty));
                    if (messages.Count > 0) return string.Join(" ", messages.Where(x => x.Length > 0));
                }

                if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    return t.GetString() ?? string.Empty;
            }
        }
        catch (JsonException)
        {
            // Not JSON (e.g. an HTML error page) - fall through to a generic message.
        }
        return string.Empty;
    }

    private static string FriendlyError(Exception ex)
    {
        if (ex is HttpRequestException || ex is TaskCanceledException || ex.InnerException is System.Net.Sockets.SocketException)
            return "Cannot connect to the API. Please make sure RaceDay.Api is running and ApiBaseUrl is correct (https://localhost:7203/).";
        return "Something went wrong while talking to the API: " + ex.Message;
    }
}
