using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class SinhVien
    {
        [Key]
        [StringLength(20)]
        public string? MaSV { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Họ Tên")]
        public string? HoTen { get; set; }

        [DisplayName("Ngày Sinh")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NgaySinh { get; set; }

        [StringLength(15)]
        [DisplayName("Điện Thoại")]
        public string? DienThoai { get; set; }

        [StringLength(255)]
        [DisplayName("Email")]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? MaLop { get; set; }

        [StringLength(20)]
        public string? MaChucVu { get; set; }

        [ForeignKey("MaLop")]
        [DisplayName("Lớp Học")]
        public LopHoc? LopHoc { get; set; }

        [ForeignKey("MaChucVu")]
        [DisplayName("Chức Vụ")]
        public ChucVu? ChucVu { get; set; }

        [StringLength(255)]
        [DisplayName("Hình Ảnh")]
        public string? HinhAnh { get; set; }
        [DisplayName("Mã QR")]
        public string? DuongdanQR { get; set; }
    }
}
