using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedLink.infrastructure.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// This corrective migration repairs a historical gap: the current EF model
    /// and snapshot require StoredFile.ProviderProfileId, its foreign key, and its
    /// index, but no tracked migration creates them.
    ///
    /// The SQL is intentionally idempotent. On an existing database where all three
    /// objects already exist, no schema or data changes are made. On a fresh
    /// database, the missing relationship is created after the prior migrations.
    ///
    /// The Down method is intentionally a no-op to avoid dropping existing data or
    /// removing a relationship that may already be in use.
    /// </remarks>
    public partial class AddStoredFileProviderProfileRelationshipIfMissing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Shared.storedFiles', 'ProviderProfileId') IS NULL
BEGIN
    ALTER TABLE [Shared].[storedFiles]
        ADD [ProviderProfileId] uniqueidentifier NULL;
END

IF EXISTS (
    SELECT 1
    FROM [Shared].[storedFiles]
    WHERE [ProviderProfileId] IS NULL
)
BEGIN
    THROW 50000, 'Cannot repair StoredFile.ProviderProfileId: existing StoredFile rows have no ProviderProfileId. Manual data mapping is required.', 1;
END

IF EXISTS (
    SELECT 1
    FROM [Shared].[storedFiles] AS s
    LEFT JOIN [Shared].[ProviderProfiles] AS p
        ON s.[ProviderProfileId] = p.[Id]
    WHERE p.[Id] IS NULL
)
BEGIN
    THROW 50000, 'Cannot repair StoredFile.ProviderProfileId: existing StoredFile rows reference missing ProviderProfiles. Manual data mapping is required.', 1;
END

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Shared.storedFiles')
      AND name = 'ProviderProfileId'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE [Shared].[storedFiles]
        ALTER COLUMN [ProviderProfileId] uniqueidentifier NOT NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('Shared.storedFiles')
      AND name = 'IX_storedFiles_ProviderProfileId'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_storedFiles_ProviderProfileId]
        ON [Shared].[storedFiles] ([ProviderProfileId]);
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_storedFiles_ProviderProfiles_ProviderProfileId'
      AND parent_object_id = OBJECT_ID('Shared.storedFiles')
)
BEGIN
    ALTER TABLE [Shared].[storedFiles]
        ADD CONSTRAINT [FK_storedFiles_ProviderProfiles_ProviderProfileId]
        FOREIGN KEY ([ProviderProfileId])
        REFERENCES [Shared].[ProviderProfiles] ([Id])
        ON DELETE CASCADE;
END
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally no-op. Automatically dropping the column, index, or foreign
            // key could remove existing relationship data and is not safe.
        }
    }
}