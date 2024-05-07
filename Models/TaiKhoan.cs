using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class TaiKhoan
    {
        [Key]
        [StringLength(20)]
        public string? MaTaiKhoan { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Tên Đăng Nhập")]
        public string? TenDangNhap { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Mật Khẩu")]
        public string? MatKhau { get; set; }

        [StringLength(50)]
        [DisplayName("Quyền")]
        public string? Quyen { get; set; }

        [StringLength(20)]
        public string? MaSV { get; set; }

        [ForeignKey("MaSV")]
        [DisplayName("Sinh Viên")]
        public SinhVien? SinhVien { get; set; }
    }
}
