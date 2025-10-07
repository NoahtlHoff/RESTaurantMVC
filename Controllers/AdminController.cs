using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly RESTaurantApiClient _apiClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(RESTaurantApiClient apiClient, ILogger<AdminController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // ===== DASHBOARD =====
        [HttpGet("")]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var bookings = await _apiClient.GetAllBookingsAsync();
                return View(bookings ?? new List<BookingVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View(new List<BookingVM>());
            }
        }

        // ===== BOOKINGS API =====
        [HttpGet("api/bookings")]
        public async Task<IActionResult> GetBookings()
        {
            try
            {
                var bookings = await _apiClient.GetAllBookingsAsync();
                if (bookings == null)
                {
                    _logger.LogWarning("GetAllBookingsAsync returned null");
                    return Ok(new List<BookingVM>());
                }

                return Ok(bookings);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when fetching bookings from API");
                return StatusCode(503, new { error = "API är inte tillgänglig", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when fetching bookings");
                return StatusCode(500, new { error = "Ett oväntat fel uppstod", details = ex.Message });
            }
        }

        [HttpGet("api/bookings/{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            try
            {
                var booking = await _apiClient.GetBookingByIdAsync(id);
                if (booking == null)
                    return NotFound(new { error = "Bokning hittades inte" });

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking {BookingId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("api/bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingVM booking)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var success = await _apiClient.CreateBookingAsync(booking);
                if (!success)
                    return StatusCode(500, new { error = "Kunde inte skapa bokning" });

                return Ok(new { message = "Bokning skapad" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("api/bookings/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingVM booking)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var success = await _apiClient.UpdateBookingAsync(id, booking);
                if (!success)
                    return NotFound(new { error = "Bokning hittades inte" });

                return Ok(new { message = "Bokning uppdaterad" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("api/bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var success = await _apiClient.DeleteBookingAsync(id);
                if (!success)
                    return NotFound(new { error = "Bokning hittades inte" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking {BookingId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ===== MENU =====
        [HttpGet("menu")]
        public async Task<IActionResult> Menu()
        {
            try
            {
                var items = await _apiClient.GetAllMenuItemsAsync();
                return View(items ?? new List<MenuItemVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu items");
                return View(new List<MenuItemVM>());
            }
        }

        [HttpGet("menu/create")]
        public IActionResult CreateMenuItem()
        {
            return View(new MenuItemVM());
        }

        [HttpPost("menu/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItemVM item)
        {
            if (!ModelState.IsValid)
                return View(item);

            try
            {
                var success = await _apiClient.CreateMenuItemAsync(item);
                if (!success)
                {
                    ModelState.AddModelError("", "Kunde inte skapa rätten.");
                    return View(item);
                }

                return RedirectToAction(nameof(Menu));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating menu item");
                ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");
                return View(item);
            }
        }

        [HttpGet("menu/edit/{id}")]
        public async Task<IActionResult> EditMenuItem(int id)
        {
            try
            {
                var item = await _apiClient.GetMenuItemByIdAsync(id);
                if (item == null)
                    return NotFound();

                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu item {MenuItemId}", id);
                return RedirectToAction(nameof(Menu));
            }
        }

        [HttpPost("menu/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(int id, MenuItemVM item)
        {
            if (!ModelState.IsValid)
                return View(item);

            try
            {
                var success = await _apiClient.UpdateMenuItemAsync(id, item);
                if (!success)
                {
                    ModelState.AddModelError("", "Kunde inte uppdatera rätten.");
                    return View(item);
                }

                return RedirectToAction(nameof(Menu));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating menu item {MenuItemId}", id);
                ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");
                return View(item);
            }
        }

        [HttpPost("menu/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var success = await _apiClient.DeleteMenuItemAsync(id);
                if (!success)
                    return NotFound();

                return RedirectToAction(nameof(Menu));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting menu item {MenuItemId}", id);
                return RedirectToAction(nameof(Menu));
            }
        }

        // ===== TABLES =====
        [HttpGet("tables")]
        public async Task<IActionResult> Tables()
        {
            try
            {
                var tables = await _apiClient.GetAllTablesAsync();
                return View(tables ?? new List<TableVM>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tables");
                return View(new List<TableVM>());
            }
        }
        [HttpGet("tables/create")]
        public IActionResult CreateTable()
        {
            return View(new TableVM());
        }

        [HttpPost("tables/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTable(TableVM table)
        {
            if (!ModelState.IsValid)
                return View(table);

            try
            {
                var success = await _apiClient.CreateTableAsync(table);
                if (!success)
                {
                    ModelState.AddModelError("", "Kunde inte skapa bordet.");
                    return View(table);
                }

                return RedirectToAction(nameof(Tables));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating table");
                ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");
                return View(table);
            }
        }

        [HttpGet("tables/edit/{id}")]
        public async Task<IActionResult> EditTable(int id)
        {
            try
            {
                var table = await _apiClient.GetTableByIdAsync(id);
                if (table == null)
                    return NotFound();

                return View(table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading table {TableId}", id);
                return RedirectToAction(nameof(Tables));
            }
        }

        [HttpPost("tables/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTable(int id, TableVM table)
        {
            if (!ModelState.IsValid)
                return View(table);

            try
            {
                var success = await _apiClient.UpdateTableAsync(id, table);
                if (!success)
                {
                    ModelState.AddModelError("", "Kunde inte uppdatera bordet.");
                    return View(table);
                }

                return RedirectToAction(nameof(Tables));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating table {TableId}", id);
                ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");
                return View(table);
            }
        }

        [HttpPost("tables/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var success = await _apiClient.DeleteTableAsync(id);
                if (!success)
                    return NotFound();

                return RedirectToAction(nameof(Tables));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting table {TableId}", id);
                return RedirectToAction(nameof(Tables));
            }
        }
    }
}