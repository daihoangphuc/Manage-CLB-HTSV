using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class DangKyHoatDong
    {
        [Key]
        [StringLength(20)]
        public string? MaDangKy { get; set; }

        [StringLength(20)]
        public string? MaSV { get; set; }

        [StringLength(20)]
        public string? MaHoatDong { get; set; }

        [DisplayName("Ngày Đăng Ký")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NgayDangKy { get; set; }

        [DisplayName("Trạng Thái Đăng Ký")]
        public bool TrangThaiDangKy { get; set; }

        [ForeignKey("MaSV")]
        [DisplayName("Sinh Viên")]
        public SinhVien? SinhVien { get; set; }

        [ForeignKey("MaHoatDong")]
        [DisplayName("Hoạt Động")]
        public HoatDong? HoatDong { get; set; }

    }
}
