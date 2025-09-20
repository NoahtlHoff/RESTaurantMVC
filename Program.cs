using Microsoft.AspNetCore.Authentication.Cookies;
using RESTaurantMVC.Services.ApiClients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => o.LoginPath = "/auth/login");
builder.Services.AddAuthorization();

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var apiBase = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBase))
    throw new InvalidOperationException("Missing Api:BaseUrl in appsettings*.json");

builder.Services.AddHttpClient<RESTaurantApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBase!);
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