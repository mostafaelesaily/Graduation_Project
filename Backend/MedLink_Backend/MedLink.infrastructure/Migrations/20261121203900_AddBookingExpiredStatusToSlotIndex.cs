using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingExpiredStatusToSlotIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_ProviderProfileId_BookingDate",
                schema: "Shared",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ProviderProfileId_BookingDate",
                schema: "Shared",
                table: "Bookings",
                columns: new[] { "ProviderProfileId", "BookingDate" },
                unique: true,
                filter: "([Status] <> 2 AND [Status] <> 4 AND [Status] <> 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_ProviderProfileId_BookingDate",
                schema: "Shared",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ProviderProfileId_BookingDate",
                schema: "Shared",
                table: "Bookings",
                columns: new[] { "ProviderProfileId", "BookingDate" },
                unique: true,
                filter: "([Status] <> 2 AND [Status] <> 4)");
        }
    }
}
