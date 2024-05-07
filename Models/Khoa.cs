using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class Khoa
    {
        [Key]
        [StringLength(20)]
        public string? MaKhoa { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tên Khoa")]
        public string? TenKhoa { get; set; }


    }
}
