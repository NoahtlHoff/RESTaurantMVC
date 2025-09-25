using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Controllers;

[Authorize]
[Route("admin")]
public class AdminController : Controller
{
    private readonly RESTaurantApiClient _api;
    public AdminController(RESTaurantApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Menu()
    {
        return View();
    }
}
