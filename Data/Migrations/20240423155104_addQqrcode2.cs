using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website_CLB_HTSV.Data.Migrations
{
    public partial class addQqrcode2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "QrId",
                table: "SinhVien",
                newName: "DuongdanQR");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DuongdanQR",
                table: "SinhVien",
                newName: "QrId");

            migrationBuilder.AddColumn<string>(
                name: "QRCodeImageQrId",
                table: "SinhVien",
                type: "nvarchar(450)",
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
    }
}
