using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordProviderServiceReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "Shared",
                table: "ProviderProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "Shared",
                table: "AppUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_AppUsers_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "Shared",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_ProviderProfiles_ProviderProfileId",
                        column: x => x.ProviderProfileId,
                        principalSchema: "Shared",
                        principalTable: "ProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProviderServices",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProviderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderServices_ProviderProfiles_ProviderProfileId",
                        column: x => x.ProviderProfileId,
                        principalSchema: "Shared",
                        principalTable: "ProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AppUsers_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "Shared",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_ProviderProfiles_ProviderProfileId",
                        column: x => x.ProviderProfileId,
                        principalSchema: "Shared",
                        principalTable: "ProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                schema: "Shared",
                table: "MedicalRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_ProviderProfileId",
                schema: "Shared",
                table: "MedicalRecords",
                column: "ProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderServices_ProviderProfileId",
                schema: "Shared",
                table: "ProviderServices",
                column: "ProviderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_PatientId",
                schema: "Shared",
                table: "Reviews",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProviderProfileId",
                schema: "Shared",
                table: "Reviews",
                column: "ProviderProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRecords",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "ProviderServices",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "Shared");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "Shared",
                table: "AppUsers");
        }
    }
}
