using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESTaurantMVC.Models;
using RESTaurantMVC.Services.ApiClients;
using System.Security.Claims;

namespace RESTaurantMVC.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly RESTaurantApiClient _api;
        public AuthController(RESTaurantApiClient api) => _api = api;

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new AdminLoginViewModel());
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var authResponse = await _api.AuthenticateAsync(model);
            if (authResponse is null || string.IsNullOrWhiteSpace(authResponse.Token))
            {
                ModelState.AddModelError(string.Empty, "Ogiltigt användarnamn eller lösenord.");
                return View(model);
            }

            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username)
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties();
            if (authResponse.ExpiresAt.HasValue)
            {
                authProperties.ExpiresUtc = authResponse.ExpiresAt;
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            HttpContext.Session.SetString(SessionKeys.ApiToken, authResponse.Token);
            _api.SetBearerToken(authResponse.Token);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove(SessionKeys.ApiToken);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _api.SetBearerToken(null);
            return RedirectToAction("Index", "Home");
        }
    }
}