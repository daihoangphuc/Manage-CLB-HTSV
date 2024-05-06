using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class LopHoc
    {
        [Key]
        [StringLength(20)]
        public string? MaLop { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tên Lớp")]
        public string? TenLop { get; set; }


        [Required]
        [StringLength(255)]
        [DisplayName("Khóa học")]
        public string? Khoahoc { get; set; }

        [Required]
        [StringLength(20)]
        public string? MaKhoa { get; set; }

        [ForeignKey("MaKhoa")]
        [DisplayName("Thuộc Khoa")]
        public Khoa? Khoa { get; set; }

    }
}
