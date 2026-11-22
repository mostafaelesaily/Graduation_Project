using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class resetofdatabaseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_storedFiles_ProviderProfiles_ProviderProfileId",
                schema: "Shared",
                table: "storedFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_storedFiles",
                schema: "Shared",
                table: "storedFiles");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.RenameTable(
                name: "storedFiles",
                schema: "Shared",
                newName: "StoredFiles",
                newSchema: "Shared");

            migrationBuilder.RenameIndex(
                name: "IX_storedFiles_ProviderProfileId",
                schema: "Shared",
                table: "StoredFiles",
                newName: "IX_StoredFiles_ProviderProfileId");

            migrationBuilder.RenameColumn(
                name: "revokedOn",
                schema: "Shared",
                table: "RefreshTokens",
                newName: "RevokedOn");

            migrationBuilder.RenameColumn(
                name: "IsActice",
                schema: "Shared",
                table: "AppUsers",
                newName: "IsActive");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StoredFiles",
                schema: "Shared",
                table: "StoredFiles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProviderServices",
                schema: "Shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                        name: "FK_Reviews_AppUsers_ProviderId",
                        column: x => x.ProviderId,
                        principalSchema: "Shared",
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderProfiles_verificationStatus",
                schema: "Shared",
                table: "ProviderProfiles",
                column: "verificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_Email",
                schema: "Shared",
                table: "AppUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                schema: "Shared",
                table: "MedicalRecords",
                column: "PatientId");

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
                name: "IX_Reviews_ProviderId",
                schema: "Shared",
                table: "Reviews",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProviderId_CreatedAt",
                schema: "Shared",
                table: "Reviews",
                columns: new[] { "ProviderId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_StoredFiles_ProviderProfiles_ProviderProfileId",
                schema: "Shared",
                table: "StoredFiles",
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
                name: "FK_StoredFiles_ProviderProfiles_ProviderProfileId",
                schema: "Shared",
                table: "StoredFiles");

            migrationBuilder.DropTable(
                name: "MedicalRecords",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "ProviderServices",
                schema: "Shared");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "Shared");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoredFiles",
                schema: "Shared",
                table: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_ProviderProfiles_verificationStatus",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_Email",
                schema: "Shared",
                table: "AppUsers");

            migrationBuilder.RenameTable(
                name: "StoredFiles",
                schema: "Shared",
                newName: "storedFiles",
                newSchema: "Shared");

            migrationBuilder.RenameIndex(
                name: "IX_StoredFiles_ProviderProfileId",
                schema: "Shared",
                table: "storedFiles",
                newName: "IX_storedFiles_ProviderProfileId");

            migrationBuilder.RenameColumn(
                name: "RevokedOn",
                schema: "Shared",
                table: "RefreshTokens",
                newName: "revokedOn");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "Shared",
                table: "AppUsers",
                newName: "IsActice");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                schema: "Shared",
                table: "ProviderProfiles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_storedFiles",
                schema: "Shared",
                table: "storedFiles",
                column: "Id");

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
    }
}
