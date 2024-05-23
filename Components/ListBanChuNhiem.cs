using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_CLB_HTSV.Data;

namespace website_CLB_HTSV.Components
{
    public class ListBanChuNhiem : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ListBanChuNhiem(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var applicationDbContext = _context.SinhVien.Include(s => s.ChucVu).Include(s => s.LopHoc).Where(s => s.ChucVu.MaChucVu != "CV04" && s.ChucVu.MaChucVu != null);
            return View("Index" , await applicationDbContext.ToListAsync());
        }
    }
}
