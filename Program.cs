using Microsoft.AspNetCore.Authentication.Cookies;
using RESTaurantMVC.Services.ApiClients;

var builder = WebApplication.CreateBuilder(args);

var apiBase = builder.Configuration["Api:BaseUrl"]!;
builder.Services.AddHttpClient<RESTaurantApiClient>(c => c.BaseAddress = new Uri(apiBase));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<RESTaurantApiClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => o.LoginPath = "/auth/login");
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
