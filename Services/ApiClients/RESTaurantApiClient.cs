using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using RESTaurantMVC.Models;

namespace RESTaurantMVC.Services.ApiClients;

public class RESTaurantApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RESTaurantApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetBearerToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = null;
            return;
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private void ApplyBearerFromSession()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.ApiToken);
        SetBearerToken(token);
    }

    // --- MENU ---
    public async Task<List<MenuItemViewModel>> GetAllMenuItemsAsync()
    {
        ApplyBearerFromSession();
        try
        {
            var result = await _http.GetAsync("api/menu-items");
            if (!result.IsSuccessStatusCode) return new();
            return await result.Content.ReadFromJsonAsync<List<MenuItemViewModel>>() ?? new();
        }
        catch { return new(); }
    }

    public async Task<List<MenuItemViewModel>> GetPopularMenuItemsAsync(int top = 6)
    {
        var all = await GetAllMenuItemsAsync();
        return all.Where(x => x.IsPopular).Take(top).ToList();
    }

    // --- AUTH ---
    public async Task<AuthTokenResponse?> AuthenticateAsync(AdminLoginViewModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", model);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        }
        catch
        {
            return null;
        }
    }
}
