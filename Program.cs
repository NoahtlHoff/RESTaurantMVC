using Microsoft.AspNetCore.Authentication.Cookies;
using RESTaurantMVC.Services.ApiClients;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/auth/login");

builder.Services.AddAuthorization();

var apiBase = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
{
    throw new InvalidOperationException("Missing Api:BaseUrl in appsettings*.json");
}

builder.Services.AddHttpClient<RESTaurantApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBase);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
