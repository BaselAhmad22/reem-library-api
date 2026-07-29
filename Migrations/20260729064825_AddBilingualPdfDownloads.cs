using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elibrary.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBilingualPdfDownloads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DownloadUrlAr",
                table: "Books",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GutenbergId",
                table: "Books",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleAr",
                table: "Books",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadUrlAr",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "GutenbergId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "TitleAr",
                table: "Books");
        }
    }
}
