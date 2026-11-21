using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAutoApply.API.Migrations
{
    /// <inheritdoc />
    public partial class AddJobMissingTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveMisses",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstMissingAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsecutiveMisses",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "FirstMissingAt",
                table: "Jobs");
        }
    }
}
