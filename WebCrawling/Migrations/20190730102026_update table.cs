using Microsoft.EntityFrameworkCore.Migrations;

namespace WebCrawling.Migrations
{
    public partial class updatetable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "EntryModels",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "EntryModels",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "EntryModels");

            migrationBuilder.DropColumn(
                name: "Site",
                table: "EntryModels");
        }
    }
}
