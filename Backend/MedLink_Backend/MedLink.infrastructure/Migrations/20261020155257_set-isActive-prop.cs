using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class setisActiveprop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderProfiles_AspNetUsers_userId",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_AppUserId",
                schema: "Shared",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                schema: "Shared",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "Shared",
                newName: "AppUsers",
                newSchema: "Shared");

            migrationBuilder.AddColumn<bool>(
                name: "IsActice",
                schema: "Shared",
                table: "AppUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUsers",
                schema: "Shared",
                table: "AppUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserClaims",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserLogins",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserRoles",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserTokens",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderProfiles_AppUsers_userId",
                schema: "Shared",
                table: "ProviderProfiles",
                column: "userId",
                principalSchema: "Shared",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AppUsers_AppUserId",
                schema: "Shared",
                table: "RefreshTokens",
                column: "AppUserId",
                principalSchema: "Shared",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AppUsers_UserId",
                schema: "Shared",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ProviderProfiles_AppUsers_userId",
                schema: "Shared",
                table: "ProviderProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_AppUsers_AppUserId",
                schema: "Shared",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUsers",
                schema: "Shared",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsActice",
                schema: "Shared",
                table: "AppUsers");

            migrationBuilder.RenameTable(
                name: "AppUsers",
                schema: "Shared",
                newName: "AspNetUsers",
                newSchema: "Shared");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                schema: "Shared",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserClaims",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserLogins",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserRoles",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                schema: "Shared",
                table: "AspNetUserTokens",
                column: "UserId",
                principalSchema: "Shared",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderProfiles_AspNetUsers_userId",
                schema: "Shared",
                table: "ProviderProfiles",
                column: "userId",
                principalSchema: "Shared",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_AspNetUsers_AppUserId",
                schema: "Shared",
                table: "RefreshTokens",
                column: "AppUserId",
                principalSchema: "Shared",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
