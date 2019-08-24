using Microsoft.EntityFrameworkCore.Migrations;

namespace LalaDb.Migrations
{
    public partial class PlaylistUsePath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackPath",
                table: "PlaylistTracks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackPath",
                table: "PlaylistTracks");
        }
    }
}
