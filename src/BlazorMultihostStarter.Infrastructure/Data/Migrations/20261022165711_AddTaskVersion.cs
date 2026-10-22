using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorMultihostStarter.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Tasks");
        }
    }
}
