using RESTaurantMVC.Models;

namespace RESTaurantMVC.Services.ApiClients
{
    public class RESTaurantApiClient
    {
        private readonly HttpClient _http;
        public RESTaurantApiClient(HttpClient http) => _http = http;

        // --- MENU ---
        public async Task<List<MenuItem>> GetPopularMenuItemsAsync(int top = 6) =>
        await _http.GetFromJsonAsync<List<MenuItem>>($"api/meny/popular?top={top}") ?? new();

        public async Task<List<MenuItem>> GetAllMenuItemsAsync() =>
        await _http.GetFromJsonAsync<List<MenuItem>>("api/meny") ?? new();
    }
}
