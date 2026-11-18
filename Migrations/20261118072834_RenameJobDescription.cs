using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAutoApply.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameJobDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Discription",
                table: "Jobs",
                newName: "Description");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Jobs",
                newName: "Discription");
        }
    }
}
