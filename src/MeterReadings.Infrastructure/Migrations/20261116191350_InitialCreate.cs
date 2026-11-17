using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeterReadings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SiteType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeterNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MeterType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meters_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeterId = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CumulativeKwh = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    IsReset = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                    table.CheckConstraint("CK_MeterReadings_CumulativeKwh", "[CumulativeKwh] >= 0");
                    table.ForeignKey(
                        name: "FK_MeterReadings_Meters_MeterId",
                        column: x => x.MeterId,
                        principalTable: "Meters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Sites",
                columns: new[] { "Id", "Address", "CreatedAt", "SiteType", "Status" },
                values: new object[] { 1, "1 Demo Street, Johannesburg", new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Commercial", "Active" });

            migrationBuilder.InsertData(
                table: "Meters",
                columns: new[] { "Id", "CreatedAt", "MeterNumber", "MeterType", "SiteId", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MTR-1001", "Electricity", 1, "Active" },
                    { 2, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MTR-1002", "Electricity", 1, "Active" }
                });

            migrationBuilder.InsertData(
                table: "MeterReadings",
                columns: new[] { "Id", "CreatedAt", "CumulativeKwh", "MeterId", "TimestampUtc" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), 12500.0m, 1, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, new DateTime(2026, 11, 1, 12, 0, 0, 0, DateTimeKind.Utc), 12521.4m, 1, new DateTime(2026, 11, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, new DateTime(2026, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 12543.7m, 1, new DateTime(2026, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4L, new DateTime(2026, 11, 2, 12, 0, 0, 0, DateTimeKind.Utc), 12560.2m, 1, new DateTime(2026, 11, 2, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 5L, new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc), 12581.9m, 1, new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6L, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc), 99950.0m, 2, new DateTime(2026, 11, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7L, new DateTime(2026, 11, 1, 12, 0, 0, 0, DateTimeKind.Utc), 99990.0m, 2, new DateTime(2026, 11, 1, 12, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "MeterReadings",
                columns: new[] { "Id", "CreatedAt", "CumulativeKwh", "IsReset", "MeterId", "TimestampUtc" },
                values: new object[] { 8L, new DateTime(2026, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc), 15.0m, true, 2, new DateTime(2026, 11, 2, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "MeterReadings",
                columns: new[] { "Id", "CreatedAt", "CumulativeKwh", "MeterId", "TimestampUtc" },
                values: new object[,]
                {
                    { 9L, new DateTime(2026, 11, 2, 12, 0, 0, 0, DateTimeKind.Utc), 40.0m, 2, new DateTime(2026, 11, 2, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 10L, new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc), 55.0m, 2, new DateTime(2026, 11, 3, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_MeterId_TimestampUtc",
                table: "MeterReadings",
                columns: new[] { "MeterId", "TimestampUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meters_MeterNumber",
                table: "Meters",
                column: "MeterNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meters_SiteId",
                table: "Meters",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeterReadings");

            migrationBuilder.DropTable(
                name: "Meters");

            migrationBuilder.DropTable(
                name: "Sites");
        }
    }
}
