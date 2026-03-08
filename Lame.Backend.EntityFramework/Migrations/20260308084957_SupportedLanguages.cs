using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lame.Backend.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SupportedLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.LanguageCode);
                });
            
            // All projects will have english by default
            migrationBuilder.InsertData(
                table: "Languages",
                column: "LanguageCode",
                values: new object[]
                {
                    "en", // English
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Languages");
        }
    }
}
