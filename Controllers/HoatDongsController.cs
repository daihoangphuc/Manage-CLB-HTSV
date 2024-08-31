using Manage_CLB_HTSV.Data;
using Manage_CLB_HTSV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Manage_CLB_HTSV.Controllers
{
    public class HoatDongsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public HoatDongsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<IActionResult> UpdateTrangThaiHoatDong()
        {
            var hoatDongs = await _context.HoatDong
                .Where(hd => hd.TrangThai != "Đã kết thúc")
                .ToListAsync(); // Truy xuất dữ liệu từ CSDL

            var currentTime = TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).Date; // Chỉ lấy phần ngày

            foreach (var hoatDong in hoatDongs)
            {
                if (hoatDong.ThoiGian.Date < currentTime)
                {
                    hoatDong.TrangThai = "Đã kết thúc";
                }
            }

            _context.UpdateRange(hoatDongs);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public string GetIDFromEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var parts = email.Split('@');
            return parts.Length > 0 ? parts[0] : email;
        }

        [HttpPost]
        public ActionResult UpdateTrangThai()
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                string updateQuery = "UPDATE HoatDong SET TrangThai = N'Sắp diễn ra' ";
                string xoatghd = "DELETE FROM ThamGiaHoatDong";
                string xoadkhd = "DELETE FROM DangKyHoatDong";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(updateQuery, connection);
                    SqlCommand command2 = new SqlCommand(xoatghd, connection);
                    SqlCommand command3 = new SqlCommand(xoadkhd, connection);

                    connection.Open();
                    command2.ExecuteNonQuery();
                    command3.ExecuteNonQuery();
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Users")]
        public async Task<IActionResult> DangKy(string hoatDongId)
        {
            if (ModelState.IsValid)
            {
                var dangKyHoatDong = new DangKyHoatDong
                {
                    MaHoatDong = hoatDongId,
                    MaSV = User.Identity.Name.Split('@')[0],
                    MaDangKy = "DK" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff"),
                    NgayDangKy = TimeZoneHelper.GetVietNamTime(DateTime.UtcNow),
                    TrangThaiDangKy = true
                };

                _context.Add(dangKyHoatDong);
                await _context.SaveChangesAsync(); // Lưu vào CSDL

                return RedirectToAction(nameof(Index));
            }

            return View(hoatDongId);
        }

        // GET: HoatDongs
        [Authorize]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            // Cập nhật trạng thái của các hoạt động trước khi hiển thị danh sách
            await UpdateTrangThaiHoatDong();

            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để truy cập vào trang này.";
                return Redirect("/Identity/Account/Login");
            }

            // Lấy thông tin người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            var Mssv = User.Identity.Name.Split('@')[0];

            IQueryable<HoatDong> hoatdongQuery;

            // Kiểm tra quyền của người dùng
            if (User.IsInRole("Administrators"))
            {
                // Quản trị viên thấy tất cả các hoạt động chưa kết thúc
                hoatdongQuery = _context.HoatDong;
            }
            else
            {
                // Người dùng bình thường thấy các hoạt động chưa đăng ký và chưa kết thúc
                hoatdongQuery = _context.HoatDong
                    .Where(hd => !_context.DangKyHoatDong
                        .Any(dk => dk.MaHoatDong == hd.MaHoatDong
                                && dk.MaSV == Mssv
                                && dk.TrangThaiDangKy == true)
                        && hd.TrangThai != "Đã kết thúc");
            }

            // Xử lý tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                if (DateTime.TryParseExact(searchString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime searchDate))
                {
                    hoatdongQuery = hoatdongQuery.Where(s => s.ThoiGian.Date == searchDate.Date);
                }
                else
                {
                    hoatdongQuery = hoatdongQuery.Where(s => s.TenHoatDong.Contains(searchString)
                                                        || s.MoTa.Contains(searchString));
                }
            }

            // Sắp xếp theo ThoiGian gần nhất lên đầu tiên
            hoatdongQuery = hoatdongQuery
                .OrderBy(hd => hd.ThoiGian); // Thay đổi thành OrderByDescending nếu muốn sắp xếp theo thứ tự giảm dần

            int pageSize = 4;

            // Tạo danh sách phân trang
            var hoatdongList = await PaginatedList<HoatDong>.CreateAsync(hoatdongQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(hoatdongList);
        }



        // GET: HoatDongs/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.HoatDong == null)
            {
                return NotFound();
            }

            var hoatDong = await _context.HoatDong
                .FirstOrDefaultAsync(m => m.MaHoatDong == id);
            if (hoatDong == null)
            {
                return NotFound();
            }

            return View(hoatDong);
        }

        // GET: HoatDongs/Create
        [Authorize(Roles = "Administrators")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: HoatDongs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Create([Bind("MaHoatDong,TenHoatDong,MoTa,ThoiGian,DiaDiem,HocKy,NamHoc,HinhAnh,TrangThai,DaDangKi,DaThamGia,MinhChung")] HoatDong hoatDong, string Toado)
        {
            if (ModelState.IsValid)
            {
                // Tách tọa độ từ chuỗi
                if (!string.IsNullOrEmpty(Toado))
                {
                    var coords = Toado.Split(',');
                    if (coords.Length == 2 &&
                        double.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) &&
                        double.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
                    {
                        hoatDong.Latitude = latitude;
                        hoatDong.Longitude = longitude;
                    }
                    else
                    {                    
                        return View(hoatDong);
                    }
                }
                else
                {
                    return View(hoatDong);
                }

                hoatDong.MaHoatDong = "HD" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff");
                hoatDong.TrangThai = "Sắp diễn ra";

                _context.Add(hoatDong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(hoatDong);
        }


        // GET: HoatDongs/Edit/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.HoatDong == null)
            {
                return NotFound();
            }

            var hoatDong = await _context.HoatDong.FindAsync(id);
            if (hoatDong == null)
            {
                return NotFound();
            }
            return View(hoatDong);
        }

        // POST: HoatDongs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("MaHoatDong,TenHoatDong,MoTa,ThoiGian,DiaDiem,HocKy,NamHoc,HinhAnh,TrangThai,DaDangKi,DaThamGia,MinhChung,Latitude,Longitude")] HoatDong hoatDong)
        {
            if (id != hoatDong.MaHoatDong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hoatDong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HoatDongExists(hoatDong.MaHoatDong))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hoatDong);
        }

        // GET: HoatDongs/Delete/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.HoatDong == null)
            {
                return NotFound();
            }

            var hoatDong = await _context.HoatDong
                .FirstOrDefaultAsync(m => m.MaHoatDong == id);
            if (hoatDong == null)
            {
                return NotFound();
            }

            return View(hoatDong);
        }

        // POST: HoatDongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.HoatDong == null)
            {
                return Problem("Entity set 'ApplicationDbContext.HoatDong'  is null.");
            }
            var hoatDong = await _context.HoatDong.FindAsync(id);
            if (hoatDong != null)
            {
                _context.HoatDong.Remove(hoatDong);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HoatDongExists(string id)
        {
            return (_context.HoatDong?.Any(e => e.MaHoatDong == id)).GetValueOrDefault();
        }
    }
}
