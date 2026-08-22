using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherEvents.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRadarScanId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ScanId",
                table: "RadarScans",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_RadarScans_ScanId",
                table: "RadarScans",
                column: "ScanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RadarScans_ScanId",
                table: "RadarScans");

            migrationBuilder.AlterColumn<string>(
                name: "ScanId",
                table: "RadarScans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
