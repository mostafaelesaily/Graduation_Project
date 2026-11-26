using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameServiceNameAndServicePhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProviderName",
                schema: "Shared",
                table: "ProviderProfiles",
                newName: "ServiceName");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "Shared",
                table: "ProviderLocations",
                newName: "ServicePhoneNumber");

            migrationBuilder.AddColumn<string>(
                name: "NationalIdNumber",
                schema: "Shared",
                table: "ProviderProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalIdNumber",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.RenameColumn(
                name: "ServiceName",
                schema: "Shared",
                table: "ProviderProfiles",
                newName: "ProviderName");

            migrationBuilder.RenameColumn(
                name: "ServicePhoneNumber",
                schema: "Shared",
                table: "ProviderLocations",
                newName: "PhoneNumber");
        }
    }
}
