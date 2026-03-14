using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lame.Backend.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class TargetAssetTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Translations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "TargetAssetTranslations",
                columns: table => new
                {
                    AssetId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    TranslationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetAssetTranslations", x => new { x.AssetId, x.Language });
                    table.ForeignKey(
                        name: "FK_TargetAssetTranslations_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TargetAssetTranslations_Translations_TranslationId",
                        column: x => x.TranslationId,
                        principalTable: "Translations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TargetAssetTranslations_TranslationId",
                table: "TargetAssetTranslations",
                column: "TranslationId");
            
            // Create entries into TargetAssetTranslations for existing translations to maintain data integrity
            migrationBuilder.Sql(@"
                INSERT INTO TargetAssetTranslations (AssetId, Language, TranslationId)
                SELECT t.AssetId, t.Language, t.Id
                FROM Translations t
                WHERE NOT EXISTS (
                    SELECT 1 FROM Translations t2
                    WHERE t2.AssetId = t.AssetId
                      AND t2.Language = t.Language
                      AND (t2.MajorVersion > t.MajorVersion
                           OR (t2.MajorVersion = t.MajorVersion AND t2.MinorVersion > t.MinorVersion))
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TargetAssetTranslations");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Translations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
