using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<MenuItemViewModel> GetMenuItemByIdAsync(int menuItemId)
    {
        try
        {
            var result = await _http.GetAsync($"api/menu-items/{menuItemId}");
            if (!result.IsSuccessStatusCode) return new();
            return await result.Content.ReadFromJsonAsync<MenuItemViewModel>() ?? new();
        }
        catch { return new(); }
    }

    public async Task<int> CreateMenuItemAsync(MenuItemViewModel newMenuItem)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/menu-items", newMenuItem);
            if (!response.IsSuccessStatusCode) return 0;
            var created = await response.Content.ReadFromJsonAsync<MenuItemViewModel>();
            return created?.Id ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<HttpStatusCode> DeleteMenuItemAsync(int menuItemId)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/menu-items/{menuItemId}");
            return response.StatusCode;
        }
        catch
        {
            return HttpStatusCode.InternalServerError;
        }
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
