using System.ComponentModel.DataAnnotations;

namespace DepEdLogbook.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LogbookIndexViewModel
    {
        public List<LogbookEntry> Entries { get; set; } = new();
        public LogbookEntryFormModel NewEntry { get; set; } = new();
        public int TotalEntries { get; set; }
        public int ReceivedToday { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalDocuments { get; set; }

        // Filters
        public DateTime? FilterFrom { get; set; }
        public DateTime? FilterTo { get; set; }
        public string? FilterDept { get; set; }
        public string? FilterDocType { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class LogbookEntryFormModel
    {
        [Required(ErrorMessage = "Department is required.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date & Time is required.")]
        public DateTime DateTimeReceived { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Received By is required.")]
        [Display(Name = "Received By")]
        public string ReceivedBy { get; set; } = string.Empty;

        public List<DocumentItemForm> Documents { get; set; } = new() { new DocumentItemForm() };
    }

    public class DocumentItemForm
    {
        // Not [Required] here — blank rows are stripped in controller before validation
        [Required(ErrorMessage = "Document Type is required.")]
        public string DocumentType { get; set; } = string.Empty;
        public string? DocCode { get; set; }
        public string Particulars { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
