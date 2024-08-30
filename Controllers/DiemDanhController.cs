using Manage_CLB_HTSV.Data;
using Manage_CLB_HTSV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manage_CLB_HTSV.Controllers
{
    public class DiemDanhController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiemDanhController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today.Date;
            var activities = await _context.HoatDong
                .Where(h => h.ThoiGian.Date == today)
                .ToListAsync();

            return View(activities);
        }
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> RecordAttendance([FromBody] AttendanceRecord model)
        {
            var nameParts = model.Name.Split('_');
            var mssv = nameParts.Last();

            // Kiểm tra xem hoạt động có tồn tại không
            var hoatdong = await _context.HoatDong.FirstOrDefaultAsync(h => h.MaHoatDong == model.MaHoatDong);
            if (hoatdong == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hoạt động." });
            }

            // Kiểm tra xem sinh viên có đăng ký hoạt động không
            var dangkihoatdong = await _context.DangKyHoatDong
                .FirstOrDefaultAsync(dk => dk.MaHoatDong == model.MaHoatDong && dk.MaSV == mssv);
            if (dangkihoatdong == null)
            {
                return Json(new { success = false, message = $"Người dùng {mssv} chưa đăng ký hoạt động {model.MaHoatDong}." });
            }

            // Kiểm tra xem sinh viên đã điểm danh cho hoạt động này chưa
            var existed = await _context.ThamGiaHoatDong
                .AnyAsync(tg => tg.MaDangKy == dangkihoatdong.MaDangKy && tg.MaHoatDong == model.MaHoatDong && tg.MaSV == mssv);
            if (existed)
            {
                return Json(new { success = false, message = $"Người dùng {dangkihoatdong.MaSV} đã điểm danh." });
            }

            // Thực hiện điểm danh
            var thamgiahoatdong = new ThamGiaHoatDong
            {
                MaThamGiaHoatDong = "TG" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff"),
                MaDangKy = dangkihoatdong.MaDangKy,
                DaThamGia = true,
                MaSV = mssv,
                MaHoatDong = model.MaHoatDong
            };

            _context.ThamGiaHoatDong.Add(thamgiahoatdong);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Điểm danh thành công." });
        }
    }
}
