using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lame.Backend.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class TranslationContentIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Assets_ContentId",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "ContentId",
                table: "Translations",
                newName: "AssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Translations_ContentId",
                table: "Translations",
                newName: "IX_Translations_AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Assets_AssetId",
                table: "Translations",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Assets_AssetId",
                table: "Translations");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "Translations",
                newName: "ContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Translations_AssetId",
                table: "Translations",
                newName: "IX_Translations_ContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Assets_ContentId",
                table: "Translations",
                column: "ContentId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
