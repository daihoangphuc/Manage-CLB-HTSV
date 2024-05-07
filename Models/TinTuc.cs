using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class TinTuc
    {
        [Key]
        [StringLength(20)]
        public string? MaTinTuc { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tiêu Đề")]
        public string? TieuDe { get; set; }

        [DisplayName("Hình Ảnh")]
        public string? HinhAnh { get; set; }

        [DisplayName("Nội Dung")]
        public string? NoiDung { get; set; }

        [DisplayName("Ngày Đăng")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime NgayDang { get; set; }

        [StringLength(20)]
      

        [ForeignKey("NguoiDang")]
        [DisplayName("Sinh Viên")]
        public string? NguoiDang { get; set; }
        public SinhVien? SinhVien { get; set; }
    }
}
