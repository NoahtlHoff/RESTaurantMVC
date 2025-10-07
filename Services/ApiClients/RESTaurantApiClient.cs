using System.Net;
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
        ApplyBearerFromSession(); // Sätt token direkt när klienten skapas
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

    // --- BOOKINGS ---
    public async Task<List<BookingVM>> GetAllBookingsAsync()
    {
        var result = await _http.GetAsync("api/bookings");
        if (!result.IsSuccessStatusCode) return new();
        return await result.Content.ReadFromJsonAsync<List<BookingVM>>() ?? new();
    }

    public async Task<BookingVM?> GetBookingByIdAsync(int id)
    {
        var result = await _http.GetAsync($"api/bookings/{id}");
        if (!result.IsSuccessStatusCode) return null;
        return await result.Content.ReadFromJsonAsync<BookingVM>();
    }

    public async Task<int> CreateBookingAsync(BookingVM booking)
    {
        var response = await _http.PostAsJsonAsync("api/bookings", booking);
        if (!response.IsSuccessStatusCode) return 0;
        var created = await response.Content.ReadFromJsonAsync<BookingVM>();
        return created?.Id ?? 0;
    }

    public async Task<bool> UpdateBookingAsync(int id, BookingVM booking)
    {
        var response = await _http.PutAsJsonAsync($"api/bookings/{id}", booking);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBookingAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/bookings/{id}");
        return response.IsSuccessStatusCode;
    }

    // --- MENU ---
    public async Task<List<MenuItemVM>> GetAllMenuItemsAsync()
    {
        var result = await _http.GetAsync("api/menu-items");
        if (!result.IsSuccessStatusCode) return new();
        return await result.Content.ReadFromJsonAsync<List<MenuItemVM>>() ?? new();
    }

    public async Task<List<MenuItemVM>> GetPopularMenuItemsAsync(int top = 6)
    {
        var all = await GetAllMenuItemsAsync();
        return all.Where(x => x.IsPopular).Take(top).ToList();
    }

    public async Task<MenuItemVM> GetMenuItemByIdAsync(int menuItemId)
    {
        var result = await _http.GetAsync($"api/menu-items/{menuItemId}");
        if (!result.IsSuccessStatusCode) return new();
        return await result.Content.ReadFromJsonAsync<MenuItemVM>() ?? new();
    }

    public async Task<int> CreateMenuItemAsync(MenuItemVM newMenuItem)
    {
        var response = await _http.PostAsJsonAsync("api/menu-items", newMenuItem);
        if (!response.IsSuccessStatusCode) return 0;
        var created = await response.Content.ReadFromJsonAsync<MenuItemVM>();
        return created?.Id ?? 0;
    }

    public async Task<HttpStatusCode> DeleteMenuItemAsync(int menuItemId)
    {
        var response = await _http.DeleteAsync($"api/menu-items/{menuItemId}");
        return response.StatusCode;
    }

    // --- AUTH ---
    public async Task<AuthTokenResponse?> AuthenticateAsync(AdminLoginViewModel model)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", model);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
    }
}