using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DepEdLogbook.Data;
using DepEdLogbook.Models;

namespace DepEdLogbook.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db) { _db = db; }

        // GET: Admin/Accounts
        public async Task<IActionResult> Accounts()
        {
            var users = await _db.Users.Where(u => u.Role != "Admin").OrderBy(u => u.Unit).ToListAsync();
            return View(users);
        }

        // GET: Admin/EditAccount/5
        public async Task<IActionResult> EditAccount(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Admin/EditAccount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccount(int id, AppUser model)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            var oldUsername = user.Username;
            user.Username = model.Username;
            user.Password = model.Password;

            _db.AuditLogs.Add(new AuditLog
            {
                Action      = "ACCOUNT_UPDATE",
                Details     = $"Admin updated account '{oldUsername}' → username: '{model.Username}'.",
                PerformedBy = User.Identity?.Name ?? "admin",
                Unit        = "ADMIN",
                Timestamp   = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Account '{user.Username}' updated successfully.";
            return RedirectToAction("Accounts");
        }

        // GET: Admin/AuditTrail
        public async Task<IActionResult> AuditTrail(int page = 1, string? filterUnit = null)
        {
            var pageSize = 20;
            var query    = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterUnit))
                query = query.Where(a => a.Unit == filterUnit);

            var total = await query.CountAsync();
            var logs  = await query.OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var units = await _db.AuditLogs.Select(a => a.Unit).Distinct().ToListAsync();

            ViewBag.Logs        = logs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.FilterUnit  = filterUnit;
            ViewBag.Units       = units;
            return View();
        }
    }
}
