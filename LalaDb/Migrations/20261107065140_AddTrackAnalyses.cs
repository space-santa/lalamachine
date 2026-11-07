using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LalaDb.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackAnalyses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackAnalyses",
                columns: table => new
                {
                    TrackAnalysisId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    FileWriteTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    AnalyzerVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    BeatsPerMinute = table.Column<double>(type: "REAL", nullable: false),
                    FirstBeat = table.Column<double>(type: "REAL", nullable: false),
                    BeatDeviationMs = table.Column<double>(type: "REAL", nullable: false),
                    BeatCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackAnalyses", x => x.TrackAnalysisId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackAnalyses_Path",
                table: "TrackAnalyses",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackAnalyses");
        }
    }
}
