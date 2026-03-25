using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lame.Backend.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RelativeFilePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Translations
                SET Content = substr(
                    Content,
                    instr(Content, 'LAME\Files\') + length('LAME\Files\')
                )
                WHERE Content LIKE '%LAME\Files\%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
