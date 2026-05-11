using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DepEdLogbook.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogbookEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTimeReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitOwner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogbookEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitFullName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogbookEntryId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Particulars = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentItems_LogbookEntries_LogbookEntryId",
                        column: x => x.LogbookEntryId,
                        principalTable: "LogbookEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Role", "Unit", "UnitFullName", "Username" },
                values: new object[,]
                {
                    { 1, "admin123", "Admin", "ADMIN", "Administrator", "admin" },
                    { 2, "Accounting2026", "Staff", "ACCOUNTING", "Accounting Unit", "AccountingUnit" },
                    { 3, "sdo123", "Staff", "SDO", "Schools Division Office", "sdo" },
                    { 4, "asds123", "Staff", "ASDS", "Assistant Schools Division Superintendent", "asds" },
                    { 5, "records123", "Staff", "RECORDS", "Records Section", "records" },
                    { 6, "hr123", "Staff", "HR", "Human Resource Section", "hr" },
                    { 7, "lr123", "Staff", "LR", "Learning Resources Section", "lr" },
                    { 8, "sgod123", "Staff", "SGOD", "School Governance and Operations Division", "sgod" },
                    { 9, "cid123", "Staff", "CID", "Curriculum and Instruction Division", "cid" },
                    { 10, "budget123", "Staff", "BUDGET", "Budget Section", "budget" },
                    { 11, "bac123", "Staff", "BAC", "Bids and Awards Committee", "bac" },
                    { 12, "cash123", "Staff", "CASH", "Cash Section", "cash" },
                    { 13, "admin_unit123", "Staff", "ADMIN_UNIT", "Administrative Division", "admin_unit" },
                    { 14, "sds123", "Staff", "SDS", "Schools Division Superintendent", "sds" },
                    { 15, "supply123", "Staff", "SUPPLY", "Supply and Property Unit", "supply" },
                    { 16, "legal123", "Staff", "LEGAL", "Legal Unit", "legal" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentItems_LogbookEntryId",
                table: "DocumentItems",
                column: "LogbookEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DocumentItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "LogbookEntries");
        }
    }
}
