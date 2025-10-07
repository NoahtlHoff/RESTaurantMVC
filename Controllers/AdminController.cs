using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Services.ApiClients;
using RESTaurantMVC.Models;

namespace RESTaurantMVC.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly RESTaurantApiClient _api;
        public AdminController(RESTaurantApiClient api) => _api = api;

        // Dashboard
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View();
        }

        // API Endpoints för JavaScript
        [HttpGet("api/bookings")]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _api.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("api/bookings/{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _api.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        [HttpPost("api/bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingVM booking)
        {
            var id = await _api.CreateBookingAsync(booking);
            if (id == 0) return BadRequest();
            return Ok(new { id });
        }

        [HttpPut("api/bookings/{id}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingVM booking)
        {
            var success = await _api.UpdateBookingAsync(id, booking);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("api/bookings/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var success = await _api.DeleteBookingAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        // Menu
        [HttpGet("menu")]
        public IActionResult Menu()
        {
            return View();
        }

        [HttpGet("menu/create")]
        public IActionResult CreateMenuItem()
        {
            return View();
        }

        [HttpGet("menu/edit")]
        public IActionResult EditMenuItem()
        {
            return View();
        }

        [HttpGet("menu/delete")]
        public IActionResult DeleteMenuItem()
        {
            return View();
        }

        // Tables
        [HttpGet("tables")]
        public IActionResult Tables()
        {
            return View();
        }

        [HttpGet("table/create")]
        public IActionResult CreateTable()
        {
            return View();
        }

        [HttpGet("table/edit")]
        public IActionResult EditTable()
        {
            return View();
        }

        [HttpGet("table/delete")]
        public IActionResult DeleteTable()
        {
            return View();
        }

        [HttpGet("table")]
        public IActionResult Table()
        {
            return View();
        }
    }
}