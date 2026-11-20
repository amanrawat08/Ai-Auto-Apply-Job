using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAutoApply.API.Migrations
{
    /// <inheritdoc />
    public partial class AddJobBoardToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoardToken",
                table: "Jobs",
                type: "nvarchar(450)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardToken",
                table: "Jobs");
        }
    }
}
