using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tracking.Infrastructure.Persistence;

#nullable disable

namespace Tracking.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(TrackingDbContext))]
    [Migration("20261122220000_AddOutboxLockedAt")]
    public partial class AddOutboxLockedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockedAt",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_LockedAt",
                table: "outbox_messages",
                column: "LockedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_LockedAt",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "outbox_messages");
        }
    }
}
