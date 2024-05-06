using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace website_CLB_HTSV.Models
{
    public class ChucVu
    {
        [Key]
        [StringLength(20)]
        public string? MaChucVu { get; set; }

        [Required]
        [StringLength(255)]
        [DisplayName("Tên Chức Vụ")]
        public string? TenChucVu { get; set; }

   
    }
}
