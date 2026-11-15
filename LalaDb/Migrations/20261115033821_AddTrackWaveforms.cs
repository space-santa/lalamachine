using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LalaDb.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackWaveforms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackWaveforms",
                columns: table => new
                {
                    TrackWaveformId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    FileWriteTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Levels = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackWaveforms", x => x.TrackWaveformId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackWaveforms_Path",
                table: "TrackWaveforms",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackWaveforms");
        }
    }
}
