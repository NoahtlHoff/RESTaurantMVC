using RESTaurantMVC.Models;

namespace RESTaurantMVC.Services.ApiClients
{
    public class RESTaurantApiClient
    {
        private readonly HttpClient _http;
        public RESTaurantApiClient(HttpClient http) => _http = http;

        // --- MENU ---
        public async Task<List<MenuItemViewModel>> GetAllMenuItemsAsync()
        {
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
    }
}
