using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LalaDb.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialKeyAndBeatsPerMinute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BeatsPerMinute",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InitialKey",
                table: "Tracks",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeatsPerMinute",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "InitialKey",
                table: "Tracks");
        }
    }
}
