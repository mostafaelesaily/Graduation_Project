using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeLocationCoordinatesNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                schema: "Shared",
                table: "ProviderLocations",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                schema: "Shared",
                table: "ProviderLocations",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldMaxLength: 255);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                schema: "Shared",
                table: "ProviderLocations",
                type: "float",
                maxLength: 255,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                schema: "Shared",
                table: "ProviderLocations",
                type: "float",
                maxLength: 255,
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}
