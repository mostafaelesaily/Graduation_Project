using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteRequirementsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "Shared",
                table: "ProviderProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM [Shared].[MedicalRecords]
    WHERE [Description] IS NOT NULL
      AND LEN([Description]) > 500
)
BEGIN
    THROW 50000, 'Cannot alter MedicalRecords.Description to nvarchar(500): existing values exceed 500 characters. Manual data cleanup is required.', 1;
END
");

            migrationBuilder.AlterColumn<string>(
                name: "Diagnosis",
                schema: "Shared",
                table: "MedicalRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Shared",
                table: "MedicalRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                schema: "Shared",
                table: "Bookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProviderAvailabilities",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderAvailabilities_ProviderProfiles_ProviderProfileId",
                        column: x => x.ProviderProfileId,
                        principalSchema: "Shared",
                        principalTable: "ProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAvailabilities_ProviderProfileId_DayOfWeek",
                schema: "Shared",
                table: "ProviderAvailabilities",
                columns: new[] { "ProviderProfileId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderAvailabilities",
                schema: "Shared");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                schema: "Shared",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Diagnosis",
                schema: "Shared",
                table: "MedicalRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Shared",
                table: "MedicalRecords",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
