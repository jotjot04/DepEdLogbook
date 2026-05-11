using Microsoft.EntityFrameworkCore;
using DepEdLogbook.Models;

namespace DepEdLogbook.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<LogbookEntry> LogbookEntries { get; set; }
        public DbSet<DocumentItem> DocumentItems { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentItem>()
                .HasOne(d => d.LogbookEntry)
                .WithMany(e => e.Documents)
                .HasForeignKey(d => d.LogbookEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default accounts
            modelBuilder.Entity<AppUser>().HasData(
                new AppUser { Id = 1,  Username = "admin",        Password = "admin123",        Role = "Admin", Unit = "ADMIN",                   UnitFullName = "Administrator" },
                new AppUser { Id = 2,  Username = "AccountingUnit", Password = "Accounting2026", Role = "Staff", Unit = "ACCOUNTING",              UnitFullName = "Accounting Unit" },
                new AppUser { Id = 3,  Username = "sdo",           Password = "sdo123",          Role = "Staff", Unit = "SDO",                     UnitFullName = "Schools Division Office" },
                new AppUser { Id = 4,  Username = "asds",          Password = "asds123",         Role = "Staff", Unit = "ASDS",                    UnitFullName = "Assistant Schools Division Superintendent" },
                new AppUser { Id = 5,  Username = "records",       Password = "records123",      Role = "Staff", Unit = "RECORDS",                 UnitFullName = "Records Section" },
                new AppUser { Id = 6,  Username = "hr",            Password = "hr123",           Role = "Staff", Unit = "HR",                      UnitFullName = "Human Resource Section" },
                new AppUser { Id = 7,  Username = "lr",            Password = "lr123",           Role = "Staff", Unit = "LR",                      UnitFullName = "Learning Resources Section" },
                new AppUser { Id = 8,  Username = "sgod",          Password = "sgod123",         Role = "Staff", Unit = "SGOD",                    UnitFullName = "School Governance and Operations Division" },
                new AppUser { Id = 9,  Username = "cid",           Password = "cid123",          Role = "Staff", Unit = "CID",                     UnitFullName = "Curriculum and Instruction Division" },
                new AppUser { Id = 10, Username = "budget",        Password = "budget123",       Role = "Staff", Unit = "BUDGET",                  UnitFullName = "Budget Section" },
                new AppUser { Id = 11, Username = "bac",           Password = "bac123",          Role = "Staff", Unit = "BAC",                     UnitFullName = "Bids and Awards Committee" },
                new AppUser { Id = 12, Username = "cash",          Password = "cash123",         Role = "Staff", Unit = "CASH",                    UnitFullName = "Cash Section" },
                new AppUser { Id = 13, Username = "admin_unit",    Password = "admin_unit123",   Role = "Staff", Unit = "ADMIN_UNIT",              UnitFullName = "Administrative Division" },
                new AppUser { Id = 14, Username = "sds",           Password = "sds123",          Role = "Staff", Unit = "SDS",                     UnitFullName = "Schools Division Superintendent" },
                new AppUser { Id = 15, Username = "supply",        Password = "supply123",       Role = "Staff", Unit = "SUPPLY",                  UnitFullName = "Supply and Property Unit" },
                new AppUser { Id = 16, Username = "legal",         Password = "legal123",        Role = "Staff", Unit = "LEGAL",                   UnitFullName = "Legal Unit" }
            );
        }
    }
}
