using System.ComponentModel.DataAnnotations;

namespace DepEdLogbook.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // "Admin" or "Staff"
        public string Role { get; set; } = "Staff";

        // Maps to department acronym e.g. "CID", "HR", "BUDGET"
        public string Unit { get; set; } = string.Empty;

        // Full department name for display
        public string UnitFullName { get; set; } = string.Empty;
    }

    public class LogbookEntry
    {
        public int Id { get; set; }

        [Required]
        public string Department { get; set; } = string.Empty;

        [Required]
        public DateTime DateTimeReceived { get; set; }

        public string ReceivedBy { get; set; } = string.Empty;

        // Which unit owns this entry
        public string UnitOwner { get; set; } = string.Empty;

        public List<DocumentItem> Documents { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class DocumentItem
    {
        public int Id { get; set; }
        public int LogbookEntryId { get; set; }

        [Required]
        public string DocumentType { get; set; } = string.Empty;

        public string? DocCode { get; set; } = string.Empty;

        [Required]
        public string Particulars { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;
        public LogbookEntry? LogbookEntry { get; set; }
    }

    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
