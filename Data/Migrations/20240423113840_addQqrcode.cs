using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website_CLB_HTSV.Data.Migrations
{
    public partial class addQqrcode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QRCodeImageQrId",
                table: "SinhVien",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrId",
                table: "SinhVien",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QRCodeImage",
                columns: table => new
                {
                    QrId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRCodeImage", x => x.QrId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_QRCodeImageQrId",
                table: "SinhVien",
                column: "QRCodeImageQrId");

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_QRCodeImage_QRCodeImageQrId",
                table: "SinhVien",
                column: "QRCodeImageQrId",
                principalTable: "QRCodeImage",
                principalColumn: "QrId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_QRCodeImage_QRCodeImageQrId",
                table: "SinhVien");

            migrationBuilder.DropTable(
                name: "QRCodeImage");

            migrationBuilder.DropIndex(
                name: "IX_SinhVien_QRCodeImageQrId",
                table: "SinhVien");

            migrationBuilder.DropColumn(
                name: "QRCodeImageQrId",
                table: "SinhVien");

            migrationBuilder.DropColumn(
                name: "QrId",
                table: "SinhVien");
        }
    }
}
