using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using website_CLB_HTSV.Data;
using website_CLB_HTSV.Models;

namespace website_CLB_HTSV.Controllers
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
                    SqlCommand command3= new SqlCommand(xoadkhd, connection);

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

            return RedirectToAction("Index"); // Chuyển hướng đến action Index hoặc một action khác nếu cần
        }
        /*private int LaySoDongTrongBang(string tenbang)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM " + tenbang;

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    int rowCount = Convert.ToInt32(command.ExecuteScalar());
                    return rowCount;
                }
            }
        }*/
        /*        public static string Taoidtaikhoan(int sequentialNumber)
                {
                    return $"DK{sequentialNumber:D3}";
                }*/

        [HttpPost]
        [Authorize(Roles = "Users")]
        public async Task<IActionResult> DangKy(string hoatDongId)
        {
            /*int y = LaySoDongTrongBang("DangKyHoatDong");*/
            if (ModelState.IsValid)
            {
                var dangKyHoatDong = new DangKyHoatDong
                {
                    MaHoatDong = hoatDongId,
                    MaSV = User.Identity.Name.Split('@')[0],
                    MaDangKy = "DK" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    NgayDangKy = DateTime.Now,
                    TrangThaiDangKy = true

                };

                _context.Add(dangKyHoatDong);
                await _context.SaveChangesAsync(); // Lưu vào CSDL

                // Điều hướng người dùng về trang cần thiết hoặc thông báo thành công
                // Ví dụ: quay trở lại danh sách hoạt động
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, quay về trang hiện tại
            return View(hoatDongId);
        }





        // GET: HoatDongs
        [Authorize]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            // Kiểm tra người dùng đã đăng nhập hay chưa
            if (!User.Identity.IsAuthenticated)
            {
                // Nếu chưa đăng nhập, đặt thông báo vào TempData
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để truy cập vào trang này.";
                return Redirect("/Identity/Account/Login");
            }

            // Lấy thông tin người dùng hiện tại
            var currentUser = await _userManager.GetUserAsync(User);
            var Mssv = User.Identity.Name.Split('@')[0];

            // Lọc các hoạt động dựa trên trạng thái đăng ký của người dùng hiện tại
            var hoatdong = _context.HoatDong
                .Where(hd => !_context.DangKyHoatDong
                    .Any(dk => dk.MaHoatDong == hd.MaHoatDong
                            && dk.MaSV == Mssv
                            && dk.TrangThaiDangKy == true) && hd.TrangThai != "Đã kết thúc");

            // Lọc theo chuỗi tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                // Chuyển đổi chuỗi ngày tháng năm đầu vào sang định dạng DateTime
                if (DateTime.TryParseExact(searchString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime searchDate))
                {
                    // Nếu chuyển đổi thành công, lọc theo ngày tháng năm
                    hoatdong = hoatdong.Where(s => s.TenHoatDong.Contains(searchString)
                                                || s.MoTa.Contains(searchString)
                                                || s.ThoiGian.Date == searchDate.Date);
                }
                else
                {
                    // Nếu không thành công, chỉ lọc theo các trường khác (tên hoạt động, mô tả)
                    hoatdong = hoatdong.Where(s => s.TenHoatDong.Contains(searchString)
                                                || s.MoTa.Contains(searchString));
                }
            }


            int pageSize = 4; // Số lượng mục trên mỗi trang
            return View(await PaginatedList<HoatDong>.CreateAsync(hoatdong.AsNoTracking(), pageNumber ?? 1, pageSize));
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Create([Bind("MaHoatDong,TenHoatDong,MoTa,ThoiGian,DiaDiem,HocKy,NamHoc,HinhAnh,TrangThai,DaDangKi,DaThamGia,MinhChung")] HoatDong hoatDong)
        {
            hoatDong.MaHoatDong = "HD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            hoatDong.TrangThai = "Sắp diễn ra";
            if (ModelState.IsValid)
            {
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("MaHoatDong,TenHoatDong,MoTa,ThoiGian,DiaDiem,HocKy,NamHoc,HinhAnh,TrangThai,DaDangKi,DaThamGia,MinhChung")] HoatDong hoatDong)
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
