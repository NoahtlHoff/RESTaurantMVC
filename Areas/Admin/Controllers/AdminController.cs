using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Areas.Admin.Controllers;

[Authorize]
[Area("admin")]
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
}
