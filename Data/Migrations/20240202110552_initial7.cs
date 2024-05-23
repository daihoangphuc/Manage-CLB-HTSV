using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website_CLB_HTSV.Data.Migrations
{
    public partial class initial7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc");

            migrationBuilder.AlterColumn<string>(
                name: "MaKhoa",
                table: "LopHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc",
                column: "MaKhoa",
                principalTable: "Khoa",
                principalColumn: "MaKhoa",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc");

            migrationBuilder.AlterColumn<string>(
                name: "MaKhoa",
                table: "LopHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc",
                column: "MaKhoa",
                principalTable: "Khoa",
                principalColumn: "MaKhoa");
        }
    }
}
