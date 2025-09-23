﻿using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;

namespace RESTaurantMVC.Areas.Admin.Controllers
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

        [HttpGet]
        [Route("menu")]
        public IActionResult Menu()
        {
            return View();
        }

        [HttpGet]
        [Route("menu-items")]
        public async Task<IActionResult> GetAllMenuItems()
        {
            var menuItems = await _api.GetAllMenuItemsAsync();
            ViewData["Title"] = "Menyn – RESTaurant";
            ViewData["Description"] = "Se hela menyn: namn, pris, beskrivning och bilder.";
            return View(menuItems);
        }

        [HttpGet]
        [Route("menu-items/popular")]
        public async Task<IActionResult> GetPopularMenuItems()
        {
            var popularMenuItems = await _api.GetPopularMenuItemsAsync();
            ViewData["Title"] = "Populära rätter – RESTaurant";
            ViewData["Description"] = "Se de mest populära rätterna.";
            return View(popularMenuItems);
        }

        [HttpGet]
        [Route("menu-items/{menuItemId:int}")]
        public async Task<IActionResult> GetMenuItemById(int menuItemId)
        {
            var menuItem = await _api.GetMenuItemByIdAsync(menuItemId);
            return View(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(MenuItemViewModel newMenuItem)
        {
            var id = await _api.CreateMenuItemAsync(newMenuItem);
            return CreatedAtAction(nameof(GetMenuItemById), new { menuItemId = id });
        }

        [HttpDelete]
        [Route("menu-items/{menuItemId:int}")]
        public async Task<IActionResult> DeleteMenuItem(int menuItemId)
        {
            var result = await _api.DeleteMenuItemAsync(menuItemId);
            if (result == System.Net.HttpStatusCode.NoContent)
                return NoContent();
            return NotFound();
        }
    }
}
