using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Controllers.Admin
{
    public class MenuController : Controller
    {
        private readonly RESTaurantApiClient _api;
        public MenuController(RESTaurantApiClient api) => _api = api;

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
