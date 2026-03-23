using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lame.Backend.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AssetLinkEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetLinks_Assets_AssetEntityId",
                table: "AssetLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetLinks_Assets_LinkedContentId",
                table: "AssetLinks");

            migrationBuilder.AddColumn<bool>(
                name: "Synced",
                table: "AssetLinks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLinks_Assets_AssetEntityId",
                table: "AssetLinks",
                column: "AssetEntityId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLinks_Assets_LinkedContentId",
                table: "AssetLinks",
                column: "LinkedContentId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetLinks_Assets_AssetEntityId",
                table: "AssetLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetLinks_Assets_LinkedContentId",
                table: "AssetLinks");

            migrationBuilder.DropColumn(
                name: "Synced",
                table: "AssetLinks");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLinks_Assets_AssetEntityId",
                table: "AssetLinks",
                column: "AssetEntityId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetLinks_Assets_LinkedContentId",
                table: "AssetLinks",
                column: "LinkedContentId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
