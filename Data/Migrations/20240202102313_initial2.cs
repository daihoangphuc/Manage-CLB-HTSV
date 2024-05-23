using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace website_CLB_HTSV.Data.Migrations
{
    public partial class initial2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoatDong_HoatDong_MaHoatDong",
                table: "DangKyHoatDong");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoatDong_SinhVien_MaSV",
                table: "DangKyHoatDong");

            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_ChucVu_MaChucVu",
                table: "SinhVien");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_LopHoc_MaLop",
                table: "SinhVien");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_SinhVien_MaSV",
                table: "TaiKhoan");

            migrationBuilder.DropForeignKey(
                name: "FK_ThamGiaHoatDong_DangKyHoatDong_MaDangKy",
                table: "ThamGiaHoatDong");

            migrationBuilder.DropForeignKey(
                name: "FK_TinTuc_SinhVien_NguoiDang",
                table: "TinTuc");

            migrationBuilder.DropIndex(
                name: "IX_TinTuc_NguoiDang",
                table: "TinTuc");

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "TinTuc",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "NguoiDang",
                table: "TinTuc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "SinhVienMaSV",
                table: "TinTuc",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaDangKy",
                table: "ThamGiaHoatDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Quyen",
                table: "TaiKhoan",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MaSV",
                table: "TaiKhoan",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MaLop",
                table: "SinhVien",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MaChucVu",
                table: "SinhVien",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SinhVien",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "DienThoai",
                table: "SinhVien",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "MaKhoa",
                table: "LopHoc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "HoatDong",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "HoatDong",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "DiaDiem",
                table: "HoatDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "MaSV",
                table: "DangKyHoatDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MaHoatDong",
                table: "DangKyHoatDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_TinTuc_SinhVienMaSV",
                table: "TinTuc",
                column: "SinhVienMaSV");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoatDong_HoatDong_MaHoatDong",
                table: "DangKyHoatDong",
                column: "MaHoatDong",
                principalTable: "HoatDong",
                principalColumn: "MaHoatDong");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoatDong_SinhVien_MaSV",
                table: "DangKyHoatDong",
                column: "MaSV",
                principalTable: "SinhVien",
                principalColumn: "MaSV");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc",
                column: "MaKhoa",
                principalTable: "Khoa",
                principalColumn: "MaKhoa");

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_ChucVu_MaChucVu",
                table: "SinhVien",
                column: "MaChucVu",
                principalTable: "ChucVu",
                principalColumn: "MaChucVu");

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_LopHoc_MaLop",
                table: "SinhVien",
                column: "MaLop",
                principalTable: "LopHoc",
                principalColumn: "MaLop");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_SinhVien_MaSV",
                table: "TaiKhoan",
                column: "MaSV",
                principalTable: "SinhVien",
                principalColumn: "MaSV");

            migrationBuilder.AddForeignKey(
                name: "FK_ThamGiaHoatDong_DangKyHoatDong_MaDangKy",
                table: "ThamGiaHoatDong",
                column: "MaDangKy",
                principalTable: "DangKyHoatDong",
                principalColumn: "MaDangKy");

            migrationBuilder.AddForeignKey(
                name: "FK_TinTuc_SinhVien_SinhVienMaSV",
                table: "TinTuc",
                column: "SinhVienMaSV",
                principalTable: "SinhVien",
                principalColumn: "MaSV");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoatDong_HoatDong_MaHoatDong",
                table: "DangKyHoatDong");

            migrationBuilder.DropForeignKey(
                name: "FK_DangKyHoatDong_SinhVien_MaSV",
                table: "DangKyHoatDong");

            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_ChucVu_MaChucVu",
                table: "SinhVien");

            migrationBuilder.DropForeignKey(
                name: "FK_SinhVien_LopHoc_MaLop",
                table: "SinhVien");

            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_SinhVien_MaSV",
                table: "TaiKhoan");

            migrationBuilder.DropForeignKey(
                name: "FK_ThamGiaHoatDong_DangKyHoatDong_MaDangKy",
                table: "ThamGiaHoatDong");

            migrationBuilder.DropForeignKey(
                name: "FK_TinTuc_SinhVien_SinhVienMaSV",
                table: "TinTuc");

            migrationBuilder.DropIndex(
                name: "IX_TinTuc_SinhVienMaSV",
                table: "TinTuc");

            migrationBuilder.DropColumn(
                name: "SinhVienMaSV",
                table: "TinTuc");

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "TinTuc",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NguoiDang",
                table: "TinTuc",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaDangKy",
                table: "ThamGiaHoatDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Quyen",
                table: "TaiKhoan",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaSV",
                table: "TaiKhoan",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaLop",
                table: "SinhVien",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaChucVu",
                table: "SinhVien",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SinhVien",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DienThoai",
                table: "SinhVien",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "HoatDong",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "HoatDong",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaDiem",
                table: "HoatDong",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaSV",
                table: "DangKyHoatDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaHoatDong",
                table: "DangKyHoatDong",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinTuc_NguoiDang",
                table: "TinTuc",
                column: "NguoiDang");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoatDong_HoatDong_MaHoatDong",
                table: "DangKyHoatDong",
                column: "MaHoatDong",
                principalTable: "HoatDong",
                principalColumn: "MaHoatDong",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyHoatDong_SinhVien_MaSV",
                table: "DangKyHoatDong",
                column: "MaSV",
                principalTable: "SinhVien",
                principalColumn: "MaSV",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_Khoa_MaKhoa",
                table: "LopHoc",
                column: "MaKhoa",
                principalTable: "Khoa",
                principalColumn: "MaKhoa",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_ChucVu_MaChucVu",
                table: "SinhVien",
                column: "MaChucVu",
                principalTable: "ChucVu",
                principalColumn: "MaChucVu",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SinhVien_LopHoc_MaLop",
                table: "SinhVien",
                column: "MaLop",
                principalTable: "LopHoc",
                principalColumn: "MaLop",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_SinhVien_MaSV",
                table: "TaiKhoan",
                column: "MaSV",
                principalTable: "SinhVien",
                principalColumn: "MaSV",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThamGiaHoatDong_DangKyHoatDong_MaDangKy",
                table: "ThamGiaHoatDong",
                column: "MaDangKy",
                principalTable: "DangKyHoatDong",
                principalColumn: "MaDangKy",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TinTuc_SinhVien_NguoiDang",
                table: "TinTuc",
                column: "NguoiDang",
                principalTable: "SinhVien",
                principalColumn: "MaSV",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
