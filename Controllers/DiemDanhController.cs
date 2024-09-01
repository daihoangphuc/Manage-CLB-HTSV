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
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var nameParts = model.Name.Split('_');
            var mssv = nameParts.Last();

            var hoatdong = await _context.HoatDong.AsNoTracking().FirstOrDefaultAsync(h => h.MaHoatDong == model.MaHoatDong);
            if (hoatdong == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hoạt động." });
            }

            var dangkihoatdong = await _context.DangKyHoatDong.AsNoTracking()
                .FirstOrDefaultAsync(dk => dk.MaHoatDong == model.MaHoatDong && dk.MaSV == mssv);
            if (dangkihoatdong == null)
            {
                return Json(new { success = false, message = $"Người dùng {mssv} chưa đăng ký hoạt động {model.MaHoatDong}." });
            }

            var existingRecords = await _context.ThamGiaHoatDong.AsNoTracking()
                .Where(tg => tg.MaHoatDong == dangkihoatdong.MaHoatDong && tg.MaDangKy == dangkihoatdong.MaDangKy && tg.MaSV == mssv)
                .ToListAsync();

            if (existingRecords.Any())
            {
                return Json(new { success = false, message = "Người dùng đã điểm danh cho hoạt động này." });
            }


            var thamgiahoatdong = new ThamGiaHoatDong
            {
                MaThamGiaHoatDong = "TG" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff"),
                MaDangKy = dangkihoatdong.MaDangKy,
                DaThamGia = true,
                MaSV = mssv,
                MaHoatDong = model.MaHoatDong
            };

            try
            {
                _context.ThamGiaHoatDong.Add(thamgiahoatdong);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Điểm danh thành công." });
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi khi lưu vào cơ sở dữ liệu
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu dữ liệu. Vui lòng thử lại." });
            }
        }
    }
}
