using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using website_CLB_HTSV.Data;

namespace website_CLB_HTSV.Components
{
        public class Avatar : ViewComponent
        {
            private readonly ApplicationDbContext _context;

            public Avatar(ApplicationDbContext context)
            {
                _context = context;
            }
        public IViewComponentResult Invoke()
        {
            string user = User.Identity.Name.Split('@')[0];
            var sinhVien = _context.SinhVien.FirstOrDefault(s => s.MaSV == user);
            return View("Index", sinhVien);
        }
    }
}
