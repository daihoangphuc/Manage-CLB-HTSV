using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using website_CLB_HTSV.Data;
using website_CLB_HTSV.Models;

namespace website_CLB_HTSV.Controllers
{
    public class TinTucsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TinTucsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Search(string keyword)
        {
            var searchResults = SearchNews(keyword);
            return PartialView("_NewsListPartial", searchResults);
        }

        private IEnumerable<TinTuc> SearchNews(string keyword)
        {
            // Thực hiện logic tìm kiếm và trả về kết quả
            // (đây chỉ là một ví dụ, bạn cần thay thế bằng logic thực tế của bạn)
            return _context.TinTuc.Where(t => t.TieuDe.Contains(keyword)).ToList();
        }
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var tinTuc = from m in _context.TinTuc
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                tinTuc = tinTuc.Where(s => s.TieuDe.Contains(searchString));
            }

            int pageSize = 5; // Số lượng mục trên mỗi trang
            return View(await PaginatedList<TinTuc>.CreateAsync(tinTuc.AsNoTracking(), pageNumber ?? 1, pageSize));
        }



        // GET: TinTucs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.TinTuc == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTuc
                .Include(t => t.SinhVien)
                .FirstOrDefaultAsync(m => m.MaTinTuc == id);
            if (tinTuc == null)
            {
                return NotFound();
            }

            return View(tinTuc);
        }

        // GET: TinTucs/Create
        [Authorize(Roles = "Administrators")]
        public IActionResult Create()
        {
            ViewData["NguoiDang"] = new SelectList(_context.SinhVien, "MaSV", "MaSV");
            return View();
        }

        // POST: TinTucs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Create([Bind("MaTinTuc,TieuDe,NoiDung,NgayDang,NguoiDang")] TinTuc tinTuc, IFormFile HinhAnh)
        {
                tinTuc.MaTinTuc = "TT" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                tinTuc.NgayDang = DateTime.Now;
                if (HinhAnh != null && HinhAnh.Length > 0)
                {
                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);

                    // Lưu file vào vị trí chỉ định
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "newsimages", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnh.CopyToAsync(stream);
                    }

                    // Cập nhật tên file cho tin tức
                    tinTuc.HinhAnh = fileName;
                }

                _context.Add(tinTuc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            ViewData["NguoiDang"] = new SelectList(_context.SinhVien, "MaSV", "MaSV", tinTuc.NguoiDang);
            return View(tinTuc);
        }

        // GET: TinTucs/Edit/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTuc.FindAsync(id);
            if (tinTuc == null)
            {
                return NotFound();
            }
            ViewData["NguoiDang"] = new SelectList(_context.SinhVien, "MaSV", "MaSV", tinTuc.NguoiDang);
            return View(tinTuc);
        }

        // POST: TinTucs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("MaTinTuc,TieuDe,NoiDung,NgayDang,NguoiDang")] TinTuc tinTuc, IFormFile HinhAnh)
        {
            if (id != tinTuc.MaTinTuc)
            {
                return NotFound();
            }


                tinTuc.NgayDang = DateTime.Now;
                try
                {
                    // Xử lý hình ảnh mới
                    if (HinhAnh != null && HinhAnh.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "newsimages", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await HinhAnh.CopyToAsync(stream);
                        }
                        // Cập nhật hình ảnh mới
                        tinTuc.HinhAnh = fileName;
                    }

                    _context.Update(tinTuc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TinTucExists(tinTuc.MaTinTuc))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            ViewData["NguoiDang"] = new SelectList(_context.SinhVien, "MaSV", "MaSV", tinTuc.NguoiDang);
            return View(tinTuc);
        }

        // GET: TinTucs/Delete/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTuc
                .Include(t => t.SinhVien)
                .FirstOrDefaultAsync(m => m.MaTinTuc == id);
            if (tinTuc == null)
            {
                return NotFound();
            }

            return View(tinTuc);
        }

        // POST: TinTucs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tinTuc = await _context.TinTuc.FindAsync(id);

            // Xóa file ảnh nếu tồn tại
            if (tinTuc.HinhAnh != null)
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "newsimages", tinTuc.HinhAnh);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.TinTuc.Remove(tinTuc);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TinTucExists(string id)
        {
            return _context.TinTuc.Any(e => e.MaTinTuc == id);
        }
    }
}