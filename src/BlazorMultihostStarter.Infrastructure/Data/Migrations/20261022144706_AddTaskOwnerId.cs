using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorMultihostStarter.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedAtUtc",
                table: "Tasks");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Tasks",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "legacy-unowned");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_OwnerId_CreatedAtUtc",
                table: "Tasks",
                columns: new[] { "OwnerId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_OwnerId_CreatedAtUtc",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tasks");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedAtUtc",
                table: "Tasks",
                column: "CreatedAtUtc");
        }
    }
}
