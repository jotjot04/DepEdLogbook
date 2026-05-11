using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DepEdLogbook.Data;
using DepEdLogbook.Models;

namespace DepEdLogbook.Controllers
{
    [Authorize(Roles = "Staff")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private const int PageSize = 10;

        public HomeController(AppDbContext db) { _db = db; }

        private string CurrentUnit => User.FindFirst("Unit")?.Value ?? "";
        private string CurrentUnitFullName => User.FindFirst("UnitFullName")?.Value ?? "";

        public async Task<IActionResult> Index(int page = 1, DateTime? filterFrom = null,
            DateTime? filterTo = null, string? filterDept = null, string? filterDocType = null)
        {
            var vm = await BuildIndexViewModel(page, filterFrom, filterTo, filterDept, filterDocType);
            vm.NewEntry = new LogbookEntryFormModel { DateTimeReceived = DateTime.Now };
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> IndexPartial(int page = 1, string? filterFrom = null,
            string? filterTo = null, string? filterDept = null, string? filterDocType = null)
        {
            DateTime? from = string.IsNullOrWhiteSpace(filterFrom) ? null : DateTime.Parse(filterFrom);
            DateTime? to   = string.IsNullOrWhiteSpace(filterTo)   ? null : DateTime.Parse(filterTo);
            var vm = await BuildIndexViewModel(page, from, to, filterDept, filterDocType);
            return Json(new
            {
                tableHtml      = RenderTableRows(vm.Entries, vm.CurrentPage, vm.PageSize),
                paginationHtml = RenderPagination(vm.CurrentPage, vm.TotalPages),
                totalCount     = vm.TotalEntries
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LogbookEntryFormModel model)
        {
            model.Documents = (model.Documents ?? new List<DocumentItemForm>())
                .Where(d => !string.IsNullOrWhiteSpace(d.Particulars) || !string.IsNullOrWhiteSpace(d.DocumentType))
                .ToList();

            if (!model.Documents.Any())
                ModelState.AddModelError("", "At least one document entry is required.");

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Documents")).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                var vm = await BuildIndexViewModel(1, null, null, null, null);
                vm.NewEntry = model;
                return View("Index", vm);
            }

            var entry = new LogbookEntry
            {
                Department       = model.Department,
                DateTimeReceived = model.DateTimeReceived,
                ReceivedBy       = model.ReceivedBy,
                UnitOwner        = CurrentUnit,
                CreatedBy        = User.Identity?.Name ?? "System",
                CreatedAt        = DateTime.Now,
                Documents        = model.Documents.Select(d => new DocumentItem
                {
                    DocumentType = d.DocumentType ?? string.Empty,
                    DocCode      = d.DocCode ?? string.Empty,
                    Particulars  = d.Particulars,
                    Remarks      = d.Remarks ?? string.Empty
                }).ToList()
            };

            _db.LogbookEntries.Add(entry);
            _db.AuditLogs.Add(new AuditLog
            {
                Action      = "CREATE",
                Details     = $"New entry for dept '{entry.Department}' received by '{entry.ReceivedBy}' with {entry.Documents.Count} doc(s).",
                PerformedBy = User.Identity?.Name ?? "Unknown",
                Unit        = CurrentUnit,
                Timestamp   = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Entry successfully added to the logbook.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var entry = await _db.LogbookEntries.Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id && e.UnitOwner == CurrentUnit);
            if (entry == null) return NotFound();

            var model = new LogbookEntryFormModel
            {
                Department       = entry.Department,
                DateTimeReceived = entry.DateTimeReceived,
                ReceivedBy       = entry.ReceivedBy,
                Documents        = entry.Documents.Select(d => new DocumentItemForm
                {
                    DocumentType = d.DocumentType,
                    DocCode      = d.DocCode,
                    Particulars  = d.Particulars,
                    Remarks      = d.Remarks
                }).ToList()
            };
            ViewBag.EntryId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LogbookEntryFormModel model)
        {
            model.Documents = (model.Documents ?? new List<DocumentItemForm>())
                .Where(d => !string.IsNullOrWhiteSpace(d.Particulars) || !string.IsNullOrWhiteSpace(d.DocumentType))
                .ToList();

            if (!model.Documents.Any())
                ModelState.AddModelError("", "At least one document entry is required.");

            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Documents")).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                ViewBag.EntryId = id;
                return View(model);
            }

            var entry = await _db.LogbookEntries.Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id && e.UnitOwner == CurrentUnit);
            if (entry == null) return NotFound();

            entry.Department       = model.Department;
            entry.DateTimeReceived = model.DateTimeReceived;
            entry.ReceivedBy       = model.ReceivedBy;

            _db.DocumentItems.RemoveRange(entry.Documents);
            entry.Documents = model.Documents.Select(d => new DocumentItem
            {
                DocumentType = d.DocumentType ?? string.Empty,
                DocCode      = d.DocCode ?? string.Empty,
                Particulars  = d.Particulars,
                Remarks      = d.Remarks ?? string.Empty
            }).ToList();

            _db.AuditLogs.Add(new AuditLog
            {
                Action      = "UPDATE",
                Details     = $"Entry #{id} updated — dept '{entry.Department}', received by '{entry.ReceivedBy}'.",
                PerformedBy = User.Identity?.Name ?? "Unknown",
                Unit        = CurrentUnit,
                Timestamp   = DateTime.Now
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Entry successfully updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entry = await _db.LogbookEntries.Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id && e.UnitOwner == CurrentUnit);
            if (entry == null) return NotFound();

            _db.AuditLogs.Add(new AuditLog
            {
                Action      = "DELETE",
                Details     = $"Entry #{id} for dept '{entry.Department}' deleted.",
                PerformedBy = User.Identity?.Name ?? "Unknown",
                Unit        = CurrentUnit,
                Timestamp   = DateTime.Now
            });

            _db.LogbookEntries.Remove(entry);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Entry successfully deleted.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AuditTrail(int page = 1)
        {
            var pageSize = 20;
            var query    = _db.AuditLogs.Where(a => a.Unit == CurrentUnit);
            var total    = await query.CountAsync();
            var logs     = await query.OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Logs        = logs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
            return View();
        }

        // ── HELPERS ──────────────────────────────────────────────────────────

        private async Task<LogbookIndexViewModel> BuildIndexViewModel(
            int page, DateTime? filterFrom, DateTime? filterTo,
            string? filterDept, string? filterDocType)
        {
            var query = _db.LogbookEntries
                .Include(e => e.Documents)
                .Where(e => e.UnitOwner == CurrentUnit)
                .AsQueryable();

            if (filterFrom.HasValue)
                query = query.Where(e => e.DateTimeReceived >= filterFrom.Value);
            if (filterTo.HasValue)
                query = query.Where(e => e.DateTimeReceived <= filterTo.Value.AddDays(1).AddSeconds(-1));
            if (!string.IsNullOrWhiteSpace(filterDept))
                query = query.Where(e => e.Department.Contains(filterDept));
            if (!string.IsNullOrWhiteSpace(filterDocType))
                query = query.Where(e => e.Documents.Any(d => d.DocumentType.Contains(filterDocType)));

            var totalFiltered = await query.CountAsync();
            var totalPages    = (int)Math.Ceiling(totalFiltered / (double)PageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var entries = await query
                .OrderByDescending(e => e.DateTimeReceived)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var today   = DateTime.Today;
            var allUnit = await _db.LogbookEntries.Include(e => e.Documents)
                .Where(e => e.UnitOwner == CurrentUnit).ToListAsync();

            return new LogbookIndexViewModel
            {
                Entries          = entries,
                TotalEntries     = allUnit.Count,
                ReceivedToday    = allUnit.Count(e => e.DateTimeReceived.Date == today),
                TotalDepartments = allUnit.Select(e => e.Department).Distinct().Count(),
                TotalDocuments   = allUnit.Sum(e => e.Documents.Count),
                FilterFrom       = filterFrom,
                FilterTo         = filterTo,
                FilterDept       = filterDept,
                FilterDocType    = filterDocType,
                CurrentPage      = page,
                TotalPages       = totalPages,
                PageSize         = PageSize
            };
        }

        private string RenderTableRows(List<LogbookEntry> entries, int currentPage, int pageSize)
        {
            if (!entries.Any())
                return "<tr><td colspan=\"9\" class=\"empty-state\"><i class=\"fas fa-inbox fa-2x\"></i><br>No logbook entries found.</td></tr>";

            var sb     = new System.Text.StringBuilder();
            int rowNum = (currentPage - 1) * pageSize + 1;

            foreach (var entry in entries)
            {
                var docCount  = entry.Documents.Count;
                var datePart  = entry.DateTimeReceived.ToString("MMM dd, yyyy");
                var timePart  = entry.DateTimeReceived.ToString("hh:mm tt");
                var dept      = System.Web.HttpUtility.HtmlEncode(entry.Department);
                var rb        = System.Web.HttpUtility.HtmlEncode(entry.ReceivedBy);
                var createdBy = System.Web.HttpUtility.HtmlEncode(entry.CreatedBy);

                for (int di = 0; di < docCount; di++)
                {
                    var doc = entry.Documents[di];
                    sb.Append($"<tr class=\"{(di % 2 == 0 ? "row-even" : "row-odd")}\">");
                    if (di == 0)
                    {
                        sb.Append($"<td rowspan=\"{docCount}\" class=\"row-num\">{rowNum}</td>");
                        sb.Append($"<td rowspan=\"{docCount}\"><span class=\"dept-badge\">{dept}</span><br><small class=\"created-by\"><i class=\"fas fa-pen fa-xs\"></i> {createdBy}</small></td>");
                        sb.Append($"<td rowspan=\"{docCount}\" class=\"datetime-cell\"><span class=\"date-part\">{datePart}</span><br><span class=\"time-part\">{timePart}</span></td>");
                        sb.Append($"<td rowspan=\"{docCount}\" class=\"received-by-cell\"><i class=\"fas fa-user-check\"></i> {rb}</td>");
                    }
                    var docType = System.Web.HttpUtility.HtmlEncode(doc.DocumentType);
                    var docCode = System.Web.HttpUtility.HtmlEncode(doc.DocCode ?? "—");
                    sb.Append($"<td><span class=\"doc-type-badge\">{docType}</span>{(string.IsNullOrEmpty(doc.DocCode) ? "" : $"<br><span class=\"doc-code-badge\">{docCode}</span>")}</td>");
                    sb.Append($"<td class=\"particulars-cell\">{System.Web.HttpUtility.HtmlEncode(doc.Particulars)}</td>");
                    sb.Append($"<td class=\"remarks-cell\">{(string.IsNullOrEmpty(doc.Remarks) ? "—" : System.Web.HttpUtility.HtmlEncode(doc.Remarks))}</td>");
                    if (di == 0)
                    {
                        sb.Append($@"<td rowspan=""{docCount}"" class=""actions-cell"">
                            <a href=""/Home/Edit/{entry.Id}"" class=""btn-edit"" title=""Edit""><i class=""fas fa-edit""></i></a>
                            <button type=""button"" class=""btn-delete"" data-delete-id=""{entry.Id}"" data-delete-dept=""{dept}"" title=""Delete""><i class=""fas fa-trash-alt""></i></button>
                            <form id=""deleteForm_{entry.Id}"" action=""/Home/Delete/{entry.Id}"" method=""post"" style=""display:none"">
                                <input name=""__RequestVerificationToken"" type=""hidden"" value="""" />
                            </form></td>");
                    }
                    sb.Append("</tr>");
                }
                rowNum++;
            }
            return sb.ToString();
        }

        // GET: Home/ExportCsv
        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? filterFrom = null, string? filterTo = null,
            string? filterDept = null, string? filterDocType = null)
        {
            DateTime? from = string.IsNullOrWhiteSpace(filterFrom) ? null : DateTime.Parse(filterFrom);
            DateTime? to   = string.IsNullOrWhiteSpace(filterTo)   ? null : DateTime.Parse(filterTo);

            var query = _db.LogbookEntries
                .Include(e => e.Documents)
                .Where(e => e.UnitOwner == CurrentUnit)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(e => e.DateTimeReceived >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.DateTimeReceived <= to.Value.AddDays(1).AddSeconds(-1));
            if (!string.IsNullOrWhiteSpace(filterDept))
                query = query.Where(e => e.Department.Contains(filterDept));
            if (!string.IsNullOrWhiteSpace(filterDocType))
                query = query.Where(e => e.Documents.Any(d => d.DocumentType.Contains(filterDocType)));

            var entries = await query.OrderByDescending(e => e.DateTimeReceived).ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("No.,Department/Office,Date Received,Time Received,Received By,Document Type,Doc Code,Particulars,Remarks");

            int rowNum = 1;
            foreach (var entry in entries)
            {
                foreach (var doc in entry.Documents)
                {
                    string Esc(string? v) => $"\"{(v ?? "").Replace("\"", "\"\"")}\"";
                    sb.AppendLine(string.Join(",",
                        rowNum,
                        Esc(entry.Department),
                        Esc(entry.DateTimeReceived.ToString("MMM dd, yyyy")),
                        Esc(entry.DateTimeReceived.ToString("hh:mm tt")),
                        Esc(entry.ReceivedBy),
                        Esc(doc.DocumentType),
                        Esc(doc.DocCode),
                        Esc(doc.Particulars),
                        Esc(doc.Remarks)    
                    ));
                }
                rowNum++;
            }

            var fromMonth = entries.Any() 
                ? entries.Min(e => e.DateTimeReceived).ToString("MMM") 
                : "Jan";
            var toMonth = entries.Any() 
                ? entries.Max(e => e.DateTimeReceived).ToString("MMM") 
                : "December";
            var fileName = $"{CurrentUnitFullName}_Logbook_{fromMonth}-{toMonth}.csv";
            var bytes = System.Text.Encoding.UTF8.GetPreamble()
                .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv", fileName);
        }

        // GET: Home/PrintPreview
        [HttpGet]
        public async Task<IActionResult> PrintPreview(string? filterFrom = null, string? filterTo = null,
            string? filterDept = null, string? filterDocType = null)
        {
            DateTime? from = string.IsNullOrWhiteSpace(filterFrom) ? null : DateTime.Parse(filterFrom);
            DateTime? to   = string.IsNullOrWhiteSpace(filterTo)   ? null : DateTime.Parse(filterTo);

            var query = _db.LogbookEntries
                .Include(e => e.Documents)
                .Where(e => e.UnitOwner == CurrentUnit)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(e => e.DateTimeReceived >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.DateTimeReceived <= to.Value.AddDays(1).AddSeconds(-1));
            if (!string.IsNullOrWhiteSpace(filterDept))
                query = query.Where(e => e.Department.Contains(filterDept));
            if (!string.IsNullOrWhiteSpace(filterDocType))
                query = query.Where(e => e.Documents.Any(d => d.DocumentType.Contains(filterDocType)));

            var entries = await query.OrderByDescending(e => e.DateTimeReceived).ToListAsync();

            ViewBag.Entries       = entries;
            ViewBag.UnitFullName  = CurrentUnitFullName;
            ViewBag.FilterFrom    = filterFrom;
            ViewBag.FilterTo      = filterTo;
            ViewBag.FilterDept    = filterDept;
            ViewBag.FilterDocType = filterDocType;
            ViewBag.PrintDate     = DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt");
            return View();
        }

        private string RenderPagination(int currentPage, int totalPages)
        {
            if (totalPages <= 1) return "";
            var sb = new System.Text.StringBuilder();
            if (currentPage > 1)
                sb.Append($"<a href=\"#logbookSection\" class=\"page-btn\" data-page=\"{currentPage - 1}\"><i class=\"fas fa-chevron-left\"></i></a>");
            for (int p = 1; p <= totalPages; p++)
                sb.Append($"<a href=\"#logbookSection\" class=\"page-btn{(p == currentPage ? " active" : "")}\" data-page=\"{p}\">{p}</a>");
            if (currentPage < totalPages)
                sb.Append($"<a href=\"#logbookSection\" class=\"page-btn\" data-page=\"{currentPage + 1}\"><i class=\"fas fa-chevron-right\"></i></a>");
            return sb.ToString();
        }
    }
}