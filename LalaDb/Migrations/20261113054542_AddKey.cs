using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LalaDb.Migrations
{
    /// <inheritdoc />
    public partial class AddKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "TrackAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "KeyAnalyzerVersion",
                table: "TrackAnalyses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "KeyAnalyzerVersion",
                table: "TrackAnalyses");
        }
    }
}
