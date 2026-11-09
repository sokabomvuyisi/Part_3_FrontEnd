var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Validation is driven by the explicit [Required]/[Range]/... attributes on the request models,
    // not by C# nullability (otherwise an empty optional Description would be rejected).
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// API base URL from config (fallback to common HTTPS ports)
var apiBase = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
    apiBase = "https://localhost:7203/";

builder.Services.AddHttpClient("RaceDayApi", client =>
{
    client.BaseAddress = new Uri(apiBase);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // IMPORTANT: no shared cookie jar. Every user's API session cookie is kept in THEIR OWN MVC
    // session and sent explicitly by ApiService, so two users can never share an API session.
    UseCookies = false,
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RaceDay.Mvc.Services.ApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // In development also show a friendlier page for unhandled errors
    app.UseExceptionHandler("/Home/Error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
