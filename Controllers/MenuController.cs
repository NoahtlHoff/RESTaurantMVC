using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Controllers
{
    public class MenuController : Controller
    {
        private readonly RESTaurantApiClient _api;
        public MenuController(RESTaurantApiClient api) => _api = api;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var menuItems = await _api.GetAllMenuItemsAsync();
            ViewData["Title"] = "Menyn – RESTaurant";
            ViewData["Description"] = "Se hela menyn: namn, pris, beskrivning och bilder.";
            return View(menuItems);
        }
    }
}
