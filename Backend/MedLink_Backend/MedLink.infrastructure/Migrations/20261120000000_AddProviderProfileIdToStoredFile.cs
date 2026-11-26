using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderProfileIdToStoredFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProviderProfileId",
                schema: "Shared",
                table: "storedFiles",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_storedFiles_ProviderProfileId",
                schema: "Shared",
                table: "storedFiles",
                column: "ProviderProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_storedFiles_ProviderProfiles_ProviderProfileId",
                schema: "Shared",
                table: "storedFiles",
                column: "ProviderProfileId",
                principalSchema: "Shared",
                principalTable: "ProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_storedFiles_ProviderProfiles_ProviderProfileId",
                schema: "Shared",
                table: "storedFiles");

            migrationBuilder.DropIndex(
                name: "IX_storedFiles_ProviderProfileId",
                schema: "Shared",
                table: "storedFiles");

            migrationBuilder.DropColumn(
                name: "ProviderProfileId",
                schema: "Shared",
                table: "storedFiles");
        }
    }
}
