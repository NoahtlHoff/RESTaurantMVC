using RESTaurantMVC.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

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
            var context = _contextAccessor.HttpContext;
            if (context is null)
            {
                return;
            }

            var token = context.Session.GetString(SessionKeys.ApiToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                token = context.User?.Claims.FirstOrDefault(c => c.Type == SessionKeys.ApiToken)?.Value;

                if (!string.IsNullOrWhiteSpace(token))
                {
                    context.Session.SetString(SessionKeys.ApiToken, token);
                }
            }

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
            if (bookings == null)
            {
                return new List<BookingVM>();
            }

            var customerCache = new Dictionary<int, CustomerDto?>();
            var tableCache = new Dictionary<int, TableVM?>();
            var result = new List<BookingVM>();

            foreach (var booking in bookings)
            {
                CustomerDto? customer = null;
                if (booking.CustomerId > 0)
                {
                    if (!customerCache.TryGetValue(booking.CustomerId, out customer))
                    {
                        customer = await GetCustomerByIdAsync(booking.CustomerId);
                        customerCache[booking.CustomerId] = customer;
                    }
                }

                TableVM? table = null;
                if (booking.TableId > 0)
                {
                    if (!tableCache.TryGetValue(booking.TableId, out table))
                    {
                        table = await GetTableByIdAsync(booking.TableId);
                        tableCache[booking.TableId] = table;
                    }
                }

                result.Add(ConvertToBookingVm(booking, customer, table));
            }

            return result;
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

        CustomerDto? customer = null;
        if (booking.CustomerId > 0)
        {
            customer = await GetCustomerByIdAsync(booking.CustomerId);
        }

        TableVM? table = null;
        if (booking.TableId > 0)
        {
            table = await GetTableByIdAsync(booking.TableId);
        }

        return ConvertToBookingVm(booking, customer, table);
    }

    private async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        EnsureToken();
        var response = await _httpClient.GetAsync($"api/customers/{id}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }

    private static BookingVM ConvertToBookingVm(BookingApiDto booking, CustomerDto? customer, TableVM? table)
    {
        return new BookingVM
        {
            Id = booking.Id,
            GuestName = Normalize(customer?.Name),
            Date = booking.StartTime.ToString("yyyy-MM-dd"),
            Time = booking.StartTime.ToString("HH:mm"),
            PartySize = booking.Guests,
            TableName = ResolveTableName(table, booking.TableId),
            Phone = ResolvePhone(customer),
            Email = Normalize(customer?.Email)
        };
    }

    private static string? ResolvePhone(CustomerDto? customer)
    {
        if (customer is null)
        {
            return null;
        }

        var phone = Normalize(customer.PhoneNumber);
        if (phone != null)
        {
            return phone;
        }

        return Normalize(customer.Phone);
    }

    private static string? ResolveTableName(TableVM? table, int tableId)
    {
        var number = Normalize(table?.Number);
        if (!string.IsNullOrWhiteSpace(number))
        {
            return number;
        }

        if (table?.Id > 0)
        {
            return table.Id.ToString();
        }

        return tableId > 0 ? tableId.ToString() : null;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
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

            return await response.Content.ReadFromJsonAsync<TableVM>();
    }

    public async Task<bool> CreateTableAsync(TableVM table)
    {
        EnsureToken();
        var response = await _httpClient.PostAsJsonAsync("api/tables", table);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTableAsync(int id, TableVM table)
    {
        EnsureToken();
        var response = await _httpClient.PutAsJsonAsync($"api/tables/{id}", table);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTableAsync(int id)
    {
        EnsureToken();
        var response = await _httpClient.DeleteAsync($"api/tables/{id}");
        return response.IsSuccessStatusCode;
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

    public class CustomerDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class CustomerCreatedDto
    {
        public int Id { get; set; }
    }
}
