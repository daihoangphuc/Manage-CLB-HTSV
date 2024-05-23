using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website_CLB_HTSV.Data.Migrations
{
    public partial class ini3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaDangKi",
                table: "HoatDong",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MinhChung",
                table: "HoatDong",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaDangKi",
                table: "HoatDong");

            migrationBuilder.DropColumn(
                name: "MinhChung",
                table: "HoatDong");
        }
    }
}
