using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DepEdLogbook.Models;
using DepEdLogbook.Data;
using Microsoft.EntityFrameworkCore;

namespace DepEdLogbook.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Accounts", "Admin");
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                model.ErrorMessage = "Invalid username or password.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Unit", user.Unit),
                new Claim("UnitFullName", user.UnitFullName),
                new Claim("UserId", user.Id.ToString())
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            _db.AuditLogs.Add(new AuditLog
            {
                Action      = "LOGIN",
                Details     = $"User '{user.Username}' ({user.UnitFullName}) logged in.",
                PerformedBy = user.Username,
                Unit        = user.Unit,
                Timestamp   = DateTime.Now
            });
            await _db.SaveChangesAsync();

            if (user.Role == "Admin")
                return RedirectToAction("Accounts", "Admin");

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name ?? "Unknown";
            var unit     = User.FindFirst("Unit")?.Value ?? "";

            _db.AuditLogs.Add(new AuditLog
            {
                Action      = "LOGOUT",
                Details     = $"User '{username}' logged out.",
                PerformedBy = username,
                Unit        = unit,
                Timestamp   = DateTime.Now
            });
            await _db.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
