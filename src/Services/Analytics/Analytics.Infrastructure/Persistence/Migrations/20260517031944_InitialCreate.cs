using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Analytics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "event_type_metrics",
                columns: table => new
                {
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_type_metrics", x => x.EventType);
                });

            migrationBuilder.CreateTable(
                name: "global_metrics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TotalProductViews = table.Column<long>(type: "bigint", nullable: false),
                    TotalOrdersCompleted = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "period_metrics",
                columns: table => new
                {
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_period_metrics", x => new { x.PeriodStart, x.EventType });
                });

            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "product_metrics",
                columns: table => new
                {
                    ProductId = table.Column<string>(type: "text", nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false),
                    AddedToCartCount = table.Column<long>(type: "bigint", nullable: false),
                    OrderCompletedCount = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_metrics", x => x.ProductId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_ProcessedAt",
                table: "processed_events",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_type_metrics");

            migrationBuilder.DropTable(
                name: "global_metrics");

            migrationBuilder.DropTable(
                name: "period_metrics");

            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropTable(
                name: "product_metrics");
        }
    }
}
