using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website_CLB_HTSV.Data.Migrations
{
    public partial class initial010420242 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkMinhChung",
                table: "ThamGiaHoatDong",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkMinhChung",
                table: "ThamGiaHoatDong");
        }
    }
}
