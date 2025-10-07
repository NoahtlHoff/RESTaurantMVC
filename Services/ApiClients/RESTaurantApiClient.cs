using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RESTaurantMVC.Models;

namespace RESTaurantMVC.Services.ApiClients
{
    public class RESTaurantApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _contextAccessor;

        public RESTaurantApiClient(HttpClient httpClient, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _contextAccessor = contextAccessor;
        }

        public void SetBearerToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private void EnsureToken()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString(SessionKeys.ApiToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                SetBearerToken(token);
            }
        }

        // AUTH
        public async Task<AuthTokenResponse?> AuthenticateAsync(AdminLoginViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                username = model.Username,
                password = model.Password
            });

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
            return result;
        }

        // MENU ITEMS
        public async Task<List<MenuItemVM>?> GetAllMenuItemsAsync()
        {
            var response = await _httpClient.GetAsync("api/menu-items");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<MenuItemVM>>();
        }

        public async Task<MenuItemVM?> GetMenuItemByIdAsync(int id)
        {
            EnsureToken();
            var response = await _httpClient.GetAsync($"api/menu-items/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<MenuItemVM>();
        }

        public async Task<bool> CreateMenuItemAsync(MenuItemVM item)
        {
            EnsureToken();
            var response = await _httpClient.PostAsJsonAsync("api/menu-items", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMenuItemAsync(int id, MenuItemVM item)
        {
            EnsureToken();
            var response = await _httpClient.PutAsJsonAsync($"api/menu-items/{id}", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            EnsureToken();
            var response = await _httpClient.DeleteAsync($"api/menu-items/{id}");
            return response.IsSuccessStatusCode;
        }

        // BOOKINGS
        public async Task<List<BookingVM>?> GetAllBookingsAsync()
        {
            try
            {
                EnsureToken();
                var response = await _httpClient.GetAsync("api/bookings");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"API returned {response.StatusCode}: {errorContent}");
                }

                var bookings = await response.Content.ReadFromJsonAsync<List<BookingApiDto>>();

                // Konvertera från API-format till VM-format
                return bookings?.Select(b => new BookingVM
                {
                    Id = b.Id,
                    GuestName = $"Kund {b.CustomerId}", // Vi har inte kundnamn direkt
                    Date = b.StartTime.ToString("yyyy-MM-dd"),
                    Time = b.StartTime.ToString("HH:mm"),
                    PartySize = b.Guests,
                    TableName = $"Bord {b.TableId}",
                    Phone = "" // API:et har inte telefon på bokning
                }).ToList();
            }
            catch (HttpRequestException)
            {
                throw; // Låt controllern hantera detta
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error fetching bookings: {ex.Message}", ex);
            }
        }

        public async Task<BookingVM?> GetBookingByIdAsync(int id)
        {
            EnsureToken();
            var response = await _httpClient.GetAsync($"api/bookings/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var booking = await response.Content.ReadFromJsonAsync<BookingApiDto>();
            if (booking == null) return null;

            return new BookingVM
            {
                Id = booking.Id,
                GuestName = $"Kund {booking.CustomerId}",
                Date = booking.StartTime.ToString("yyyy-MM-dd"),
                Time = booking.StartTime.ToString("HH:mm"),
                PartySize = booking.Guests,
                TableName = $"Bord {booking.TableId}"
            };
        }

        public async Task<bool> CreateBookingAsync(BookingVM booking)
        {
            EnsureToken();

            // Konvertera från VM till API-format
            var startTime = DateTime.Parse($"{booking.Date} {booking.Time}");

            var apiBooking = new
            {
                startTime = startTime,
                guests = booking.PartySize,
                customerId = 1, // Hårdkodat för nu - skulle behöva skapas först
                tableId = 1 // Hårdkodat för nu - skulle behöva väljas från tillgängliga
            };

            var response = await _httpClient.PostAsJsonAsync("api/bookings", apiBooking);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateBookingAsync(int id, BookingVM booking)
        {
            EnsureToken();

            var startTime = DateTime.Parse($"{booking.Date} {booking.Time}");

            var apiBooking = new
            {
                startTime = startTime,
                guests = booking.PartySize,
                customerId = 1,
                tableId = 1
            };

            var response = await _httpClient.PutAsJsonAsync($"api/bookings/{id}", apiBooking);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            EnsureToken();
            var response = await _httpClient.DeleteAsync($"api/bookings/{id}");
            return response.IsSuccessStatusCode;
        }

        // TABLES
        public async Task<List<TableVM>?> GetAllTablesAsync()
        {
            EnsureToken();
            var response = await _httpClient.GetAsync("api/tables");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<TableVM>>();
        }

        // CUSTOMERS (för att kunna skapa bokningar)
        public async Task<int?> CreateCustomerAsync(string name, string phone)
        {
            EnsureToken();

            var customer = new { name, phone };
            var response = await _httpClient.PostAsJsonAsync("api/customers", customer);

            if (!response.IsSuccessStatusCode)
                return null;

            var created = await response.Content.ReadFromJsonAsync<CustomerCreatedDto>();
            return created?.Id;
        }
    }

    // DTO:er för API-kommunikation
    public class BookingApiDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int Guests { get; set; }
        public int CustomerId { get; set; }
        public int TableId { get; set; }
    }

    public class TableVM
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public int Capacity { get; set; }
    }

    public class CustomerCreatedDto
    {
        public int Id { get; set; }
    }
}