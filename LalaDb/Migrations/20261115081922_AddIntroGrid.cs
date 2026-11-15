using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LalaDb.Migrations
{
    /// <inheritdoc />
    public partial class AddIntroGrid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntroBeatCount",
                table: "TrackAnalyses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "IntroBeatDeviationMs",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IntroBeatsPerMinute",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IntroFirstBeat",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IntroFirstDownbeat",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IntroMusicEnd",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "IntroMusicStart",
                table: "TrackAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntroBeatCount",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "IntroBeatDeviationMs",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "IntroBeatsPerMinute",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "IntroFirstBeat",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "IntroFirstDownbeat",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "IntroMusicEnd",
                table: "TrackAnalyses");

            migrationBuilder.DropColumn(
                name: "IntroMusicStart",
                table: "TrackAnalyses");
        }
    }
}
