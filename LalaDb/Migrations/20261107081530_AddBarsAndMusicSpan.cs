using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LalaDb.Migrations
{
    /// <inheritdoc />
    public partial class AddBarsAndMusicSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FirstDownbeat",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MusicEnd",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MusicStart",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstDownbeat",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "MusicEnd",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "MusicStart",
                table: "TrackAnalyses");
        }
    }
}
