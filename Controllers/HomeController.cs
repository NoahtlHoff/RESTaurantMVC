using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;
using System.Diagnostics;

namespace RESTaurantMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RESTaurantApiClient _api;
        public HomeController(ILogger<HomeController> logger, RESTaurantApiClient api)
        {
            _logger = logger;
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _api.GetPopularMenuItemsAsync(6);
            return View(featured);
        }

        [HttpGet]
        public async Task<IActionResult> Menu()
        {
            var menuItems = await _api.GetAllMenuItemsAsync();
            ViewData["Title"] = "Menyn – RESTaurant";
            ViewData["Description"] = "Se hela menyn: namn, pris, beskrivning och bilder.";
            return View(menuItems);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
