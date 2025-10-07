using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly RESTaurantApiClient _apiClient;

        public HomeController(RESTaurantApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var allItems = await _apiClient.GetAllMenuItemsAsync();
            var popularItems = allItems?
                .Where(item => item.IsPopular)
                .Take(6)
                .ToList() ?? new List<MenuItemVM>();

            return View(popularItems);
        }
        [HttpGet("meny")]
        public async Task<IActionResult> Menu()
        {
            var items = await _apiClient.GetAllMenuItemsAsync();
            return View(items ?? new List<MenuItemVM>());
        }
        public IActionResult Booking()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorVM
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}