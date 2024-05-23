using Microsoft.AspNetCore.Mvc;
using website_CLB_HTSV.Data;

namespace website_CLB_HTSV.Components
{
    public class ListTinTuc : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ListTinTuc(ApplicationDbContext context)
        {
            _context = context;
        }
        public IViewComponentResult Invoke ()
        {
            return View(_context.TinTuc.ToList());
        }
    }
}
