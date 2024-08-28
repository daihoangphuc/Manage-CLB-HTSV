using Manage_CLB_HTSV.Data;
using Manage_CLB_HTSV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
namespace Manage_CLB_HTSV.Controllers
{
    public class DangKyHoatDongsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DangKyHoatDongsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DiemDanh(string mahd, double kinhdo, double vido)
        {
            var mssv = User.Identity.Name.Split('@')[0];

            // Lấy thông tin hoạt động
            var hoatdong = await _context.HoatDong.FirstOrDefaultAsync(h => h.MaHoatDong == mahd);
            if (hoatdong == null)
            {
                return RedirectToAction(nameof(Index), new { errorMessage = "Không tìm thấy hoạt động." });
            }
            // Ghi lại tọa độ cho debug
            Console.WriteLine($"Hoat dong: Latitude = {hoatdong.Latitude}, Longitude = {hoatdong.Longitude}");
            Console.WriteLine($"User: Latitude = {vido}, Longitude = {kinhdo}");
            // Lấy thông tin đăng ký hoạt động
            var dangkihoatdong = await _context.DangKyHoatDong
                .FirstOrDefaultAsync(dk => dk.MaHoatDong == mahd && dk.MaSV == mssv);
            if (dangkihoatdong == null)
            {
                return RedirectToAction(nameof(Index), new { errorMessage = "Bạn chưa đăng ký hoạt động này." });
            }

            // Kiểm tra khoảng cách điểm danh
            if (IsWithinAcceptableDistance((double)hoatdong.Latitude, (double)hoatdong.Longitude, vido, kinhdo))
            {
                var thamgiahoatdong = new ThamGiaHoatDong
                {
                    MaThamGiaHoatDong = "TG" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff"),
                    MaDangKy = dangkihoatdong.MaDangKy,
                    DaThamGia = true,
                    MaSV = mssv,
                    MaHoatDong = mahd
                };
                _context.ThamGiaHoatDong.Add(thamgiahoatdong);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage_V_DiemDanh"] = "Điểm danh thành công.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage_V_DiemDanh"] = $"Điểm danh thất bại. Bạn không ở gần địa điểm hoạt động.";
                //\n Tọa độ của bạn là {vido}, {kinhdo} 
                return RedirectToAction(nameof(Index));
            }
        }

        public bool IsWithinAcceptableDistance(double activityLatitude, double activityLongitude, double checkLatitude, double checkLongitude, double acceptableDistanceInMeters = 200)
        {
            const double EarthRadiusKm = 6371.0;

            double lat1 = ToRadians(activityLatitude);
            double lon1 = ToRadians(activityLongitude);
            double lat2 = ToRadians(checkLatitude);
            double lon2 = ToRadians(checkLongitude);

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1) * Math.Cos(lat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = EarthRadiusKm * c * 1000; // Đổi từ km sang mét

            // Log khoảng cách để debug
            Console.WriteLine($"Khoảng cách tính toán: {distance} mét. Khoảng cách chấp nhận được: {acceptableDistanceInMeters} mét.");

            return distance <= acceptableDistanceInMeters;
        }


        private double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        //Cập nhật minh chứng 
        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> CapNhatMinhChung()
        {
            var activityList = await _context.HoatDong.Where(t=>t.TrangThai == "Đã kết thúc")
                                             .Select(a => new SelectListItem
                                             {
                                                 Value = a.MaHoatDong.ToString(),
                                                 Text = a.TenHoatDong
                                             })
                                             .ToListAsync();

            ViewBag.ActivityList = activityList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> CapNhatMinhChung(string hoatDongId, string minhChungLink)
        {
            if (string.IsNullOrEmpty(hoatDongId) || string.IsNullOrEmpty(minhChungLink))
            {
                TempData["ErrorMessage"] = "Thông tin hoặc đường dẫn minh chứng không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy danh sách sinh viên đã tham gia hoạt động
            var registeredStudents = await _context.ThamGiaHoatDong
                .Where(dk => dk.MaHoatDong == hoatDongId)
                .ToListAsync();


            if (registeredStudents == null || !registeredStudents.Any())
            {
                TempData["ErrorMessage"] = "Không có sinh viên nào đã đăng ký hoạt động này.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var registeredStudent in registeredStudents)
            {
                 registeredStudent.LinkMinhChung = minhChungLink;

                _context.ThamGiaHoatDong.Update(registeredStudent);
            }
            // Cập nhật trạng thái hoạt động nếu link minh chứng không rỗng


            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật minh chứng cho tất cả sinh viên thành công.";
            // Lấy danh sách email từ bảng SinhVien
            var sinhViens = (from sv in _context.SinhVien
                             join dk in _context.DangKyHoatDong on sv.MaSV equals dk.MaSV
                             join tg in _context.ThamGiaHoatDong on dk.MaDangKy equals tg.MaDangKy
                             where tg.MaHoatDong == hoatDongId && tg.DaThamGia
                             select sv.Email).Distinct().ToArray();


            // Đường dẫn đến file HTML template
            string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "emailhtml", "email_template.html");

            // Đọc nội dung của file template
            string htmlTemplate = System.IO.File.ReadAllText(templatePath);

            // Sử dụng danh sách email
            string[] recipients = sinhViens;
            var hdong = _context.HoatDong.FirstOrDefault(h=>h.MaHoatDong == hoatDongId);

            string subject = $"Hoạt động {hdong?.TenHoatDong} đã có minh chứng!";
            // Thực hiện thay thế các giá trị mong muốn trong template
            string name = hdong.TenHoatDong.ToString();
            string tgian = hdong.ThoiGian.ToShortTimeString();
            string ngaytc = hdong.ThoiGian.ToLongDateString();
            string ddiem = hdong.DiaDiem.ToString();
            string htmlMessage = htmlTemplate.Replace("{{TenHoatDong}}", name).Replace("{{ThoiGian}}", tgian).Replace("{{NgayToChuc}}", ngaytc).Replace("{{DiaDiem}}", ddiem);


     

            try
            {
                await _emailSender.SendEmailsAsync(recipients, subject, htmlMessage);
                return RedirectToAction("Index", "Home"); // Redirect to success page
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                
            }
            return RedirectToAction("Index", "ThamGiaHoatDongs");
        }




        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> ImportDiemDanh()
        {
            var activityList = await _context.HoatDong.Where(t => t.TrangThai == "Sắp diễn ra")
                                             .Select(a => new SelectListItem
                                             {
                                                 Value = a.MaHoatDong.ToString(),
                                                 Text = a.TenHoatDong
                                             })
                                             .ToListAsync();

            ViewBag.ActivityList = activityList;

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> ImportDiemDanh(IFormFile file, string hoatDongId)
        {
            if (file == null || string.IsNullOrEmpty(hoatDongId))
            {
                TempData["ErrorMessage"] = "File hoặc hoạt động không hợp lệ.";
                return RedirectToAction(nameof(ImportDiemDanh)); // Chuyển hướng trở lại action ImportDiemDanh
            }

            var importedStudentCodes = ReadStudentCodesFromExcel(file);

            if (importedStudentCodes == null || !importedStudentCodes.Any())
            {
                TempData["ErrorMessage"] = "Không có dữ liệu trong file.";
                return RedirectToAction(nameof(ImportDiemDanh)); // Chuyển hướng trở lại action ImportDiemDanh
            }

            // Lấy danh sách sinh viên đã đăng ký cho hoạt động
            var registeredStudents = await _context.DangKyHoatDong
                                                .Where(dk => dk.MaHoatDong == hoatDongId)
                                                .ToListAsync();
            var hoatDong = await _context.HoatDong.FirstOrDefaultAsync(h => h.MaHoatDong == hoatDongId);
            List<string> matchedStudentCodes = new List<string>(); // Danh sách mã sinh viên khớp
            foreach (var registeredStudent in registeredStudents)
            {
                if (importedStudentCodes.Contains(registeredStudent.MaSV))
                {
                    matchedStudentCodes.Add(registeredStudent.MaSV); // Thêm vào danh sách mã sinh viên khớp
                                                                     // Kiểm tra xem sinh viên đã điểm danh chưa, nếu chưa thì cập nhật
                    var attendanceRecord = await _context.ThamGiaHoatDong
                                                        .FirstOrDefaultAsync(t => t.MaDangKy == registeredStudent.MaDangKy);

                    if (attendanceRecord == null)
                    {
                        var newAttendanceRecord = new ThamGiaHoatDong
                        {
                            MaThamGiaHoatDong = "TG" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff"),
                            MaDangKy = registeredStudent.MaDangKy,
                            DaThamGia = true,
                            MaSV = registeredStudent.MaSV,
                            MaHoatDong = registeredStudent.MaHoatDong
                        };
                        hoatDong.TrangThai = "Đã kết thúc"; // Cập nhật trạng thái hoạt động là true
                        _context.HoatDong.Update(hoatDong);

                        _context.ThamGiaHoatDong.Add(newAttendanceRecord);
                    }
                    else
                    {
                        // Nếu sinh viên đã tồn tại trong danh sách điểm danh, cập nhật thuộc tính DaThamGia
                        attendanceRecord.DaThamGia = true;
                    }
                }
                else
                {
                    // Nếu sinh viên không khớp, thêm mới một bản ghi vào ThamGiaHoatDong với DaThamGia là false
                    var newAttendanceRecord = new ThamGiaHoatDong
                    {
                        MaThamGiaHoatDong = "TG" + TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("yyyyMMddHHmmssfff"),
                        MaDangKy = registeredStudent.MaDangKy,
                        DaThamGia = false, // Đánh dấu là chưa tham gia
                        MaSV = registeredStudent.MaSV,
                        MaHoatDong = registeredStudent.MaHoatDong
                    };
                    _context.ThamGiaHoatDong.Add(newAttendanceRecord);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã import và cập nhật điểm danh thành công.";
            return RedirectToAction("Index", "ThamGiaHoatDongs"); // Hoặc trả về một View cụ thể
        }


        [Authorize(Roles = "Administrators")]
        private List<string> ReadStudentCodesFromExcel(IFormFile file)
        {
            List<string> studentCodes = new List<string>();
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++)
                    {
                        studentCodes.Add(worksheet.Cells[row, 1].Value?.ToString().Trim());
                    }
                }
            }
            return studentCodes;
        }



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> HuyDangKy(string hoatDongId)
        {
            var mssv = User.Identity.Name.Split('@')[0];

            // Tìm mục DangKyHoatDong tương ứng với hoatDongId và mssv và trạng thái đăng ký là true
            var dangKyHoatDong = await _context.DangKyHoatDong
                .FirstOrDefaultAsync(dk => dk.MaHoatDong == hoatDongId
                                        && dk.MaSV == mssv
                                        && dk.TrangThaiDangKy == true);

            if (dangKyHoatDong != null)
            {
                
                try
                {
                    // Xóa mục DangKyHoatDong từ cơ sở dữ liệu
                    _context.DangKyHoatDong.Remove(dangKyHoatDong);
                    await _context.SaveChangesAsync();

                    // Điều hướng người dùng về trang cần thiết hoặc thông báo thành công
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Xử lý ngoại lệ nếu có
                    // Ở đây, bạn có thể trả về một thông báo lỗi hoặc điều hướng người dùng đến một trang lỗi
                    return RedirectToAction(nameof(Error));
                }
            }
            else
            {
                // Trả về NotFound nếu không tìm thấy mục tương ứng
                return NotFound();
            }
        }

        private object Error()
        {
            throw new NotImplementedException();
        }


        // GET: DangKyHoatDongs
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

            IQueryable<DangKyHoatDong> hoatdong;

            // Nếu người dùng không phải là quản trị viên
            if (!User.IsInRole("Administrators"))
            {
                // Lọc danh sách hoạt động theo người dùng hiện tại
                hoatdong = _context.DangKyHoatDong
                    .Include(s => s.HoatDong)
                    .Include(s => s.SinhVien)
                    .Where(dk => dk.MaSV == Mssv && dk.TrangThaiDangKy == true);
            }
            else
            {
                // Lấy tất cả các đăng ký hoạt động nếu là quản trị viên
                hoatdong = _context.DangKyHoatDong
                    .Include(s => s.HoatDong)
                    .Include(s => s.SinhVien)
                    .Where(dk => dk.TrangThaiDangKy == true);
            }

            // Lọc theo chuỗi tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                hoatdong = hoatdong
                    .Where(s => s.MaHoatDong.Contains(searchString) || s.MaSV.Contains(searchString));
            }

            // Sắp xếp theo NgàyĐăngKý gần nhất lên đầu tiên
            hoatdong = hoatdong
                .OrderByDescending(dk => dk.NgayDangKy);

            // Số lượng mục trên mỗi trang
            int pageSize = 10;

            // Tạo danh sách phân trang từ danh sách đăng ký hoạt động
            var paginatedDangKyHoatDongs = await PaginatedList<DangKyHoatDong>.CreateAsync(hoatdong.AsNoTracking(), pageNumber ?? 1, pageSize);

            // Lấy trạng thái "Đã Tham Gia" của mỗi hoạt động
            var thamGiaHoatDong = _context.ThamGiaHoatDong
                .Where(tg => tg.MaSV == Mssv)
                .ToDictionary(tg => tg.MaHoatDong, tg => tg.DaThamGia);

            // Truyền danh sách trạng thái "Đã Tham Gia" vào ViewBag
            ViewBag.ThamGiaStatuses = thamGiaHoatDong;

            return View(paginatedDangKyHoatDongs);
        }


        // GET: DangKyHoatDongs/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.DangKyHoatDong == null)
            {
                return NotFound();
            }

            var dangKyHoatDong = await _context.DangKyHoatDong
                .Include(d => d.HoatDong)
                .Include(d => d.SinhVien)
                .FirstOrDefaultAsync(m => m.MaDangKy == id);
            if (dangKyHoatDong == null)
            {
                return NotFound();
            }

            return View(dangKyHoatDong);
        }

        // GET: DangKyHoatDongs/Create
        [Authorize(Roles = "Administrators")]
        public IActionResult Create()
        {
            ViewData["MaHoatDong"] = new SelectList(_context.HoatDong, "MaHoatDong", "MaHoatDong");
            ViewData["MaSV"] = new SelectList(_context.SinhVien, "MaSV", "MaSV");
            return View();
        }

        // POST: DangKyHoatDongs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Create([Bind("MaDangKy,MaSV,MaHoatDong,NgayDangKy,TrangThaiDangKy")] DangKyHoatDong dangKyHoatDong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dangKyHoatDong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaHoatDong"] = new SelectList(_context.HoatDong, "MaHoatDong", "MaHoatDong", dangKyHoatDong.MaHoatDong);
            ViewData["MaSV"] = new SelectList(_context.SinhVien, "MaSV", "MaSV", dangKyHoatDong.MaSV);
            return View(dangKyHoatDong);
        }

        // GET: DangKyHoatDongs/Edit/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.DangKyHoatDong == null)
            {
                return NotFound();
            }

            var dangKyHoatDong = await _context.DangKyHoatDong.FindAsync(id);
            if (dangKyHoatDong == null)
            {
                return NotFound();
            }
            ViewData["MaHoatDong"] = new SelectList(_context.HoatDong, "MaHoatDong", "MaHoatDong", dangKyHoatDong.MaHoatDong);
            ViewData["MaSV"] = new SelectList(_context.SinhVien, "MaSV", "MaSV", dangKyHoatDong.MaSV);
            return View(dangKyHoatDong);
        }

        // POST: DangKyHoatDongs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("MaDangKy,MaSV,MaHoatDong,NgayDangKy,TrangThaiDangKy")] DangKyHoatDong dangKyHoatDong)
        {
            if (id != dangKyHoatDong.MaDangKy)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dangKyHoatDong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DangKyHoatDongExists(dangKyHoatDong.MaDangKy))
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
            ViewData["MaHoatDong"] = new SelectList(_context.HoatDong, "MaHoatDong", "MaHoatDong", dangKyHoatDong.MaHoatDong);
            ViewData["MaSV"] = new SelectList(_context.SinhVien, "MaSV", "MaSV", dangKyHoatDong.MaSV);
            return View(dangKyHoatDong);
        }

        // GET: DangKyHoatDongs/Delete/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.DangKyHoatDong == null)
            {
                return NotFound();
            }

            var dangKyHoatDong = await _context.DangKyHoatDong
                .Include(d => d.HoatDong)
                .Include(d => d.SinhVien)
                .FirstOrDefaultAsync(m => m.MaDangKy == id);
            if (dangKyHoatDong == null)
            {
                return NotFound();
            }

            return View(dangKyHoatDong);
        }

        // POST: DangKyHoatDongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.DangKyHoatDong == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DangKyHoatDong'  is null.");
            }
            var dangKyHoatDong = await _context.DangKyHoatDong.FindAsync(id);
            if (dangKyHoatDong != null)
            {
                _context.DangKyHoatDong.Remove(dangKyHoatDong);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DangKyHoatDongExists(string id)
        {
          return (_context.DangKyHoatDong?.Any(e => e.MaDangKy == id)).GetValueOrDefault();
        }
    }
}
