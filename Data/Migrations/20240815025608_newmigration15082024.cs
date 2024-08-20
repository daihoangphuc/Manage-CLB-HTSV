using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manage_CLB_HTSV.Data.Migrations
{
    public partial class newmigration15082024 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "HoatDong",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "HoatDong",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "HoatDong");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "HoatDong");
        }
    }
}
