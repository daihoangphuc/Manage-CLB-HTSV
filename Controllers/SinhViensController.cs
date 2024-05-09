using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SendGrid.Helpers.Errors.Model;
using website_CLB_HTSV.Data;
using website_CLB_HTSV.Models;

namespace website_CLB_HTSV.Controllers
{
    public class SinhViensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SinhViensController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task CreateOrUpdateQRCodeImageAsync(SinhVien sinhVien)
        {
            var qrContent = $"{sinhVien.Email.Split('@')[0].ToString()}"; // Thông tin bạn muốn chứa trong mã QR

            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = 300,
                    Width = 300
                }
            };

            var pixelData = qrCodeWriter.Write(qrContent);

            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                using (var ms = new MemoryStream())
                {
                    var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    try
                    {
                        // Copy the bytes from the pixel data to the bitmap
                        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0,
                            pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    // Tạo tên tệp duy nhất cho hình ảnh mã QR
                    var fileName = $"{sinhVien.MaSV.ToString()}.png";
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "qrcode", fileName); // Lưu vào thư mục "qrcode" trong wwwroot
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

                    // Lưu tên tệp vào cơ sở dữ liệu
                    sinhVien.DuongdanQR = fileName;
                    _context.Update(sinhVien);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public IActionResult UpdateProfile()
        {
            if (!User.IsInRole("Administrators"))
            {
                var currentUserId = User.Identity.Name.Split('@')[0];
                var sinhVien = _context.SinhVien.Find(currentUserId);

                ViewData["MaChucVu"] = new SelectList(_context.ChucVu, "MaChucVu", "TenChucVu", "CV04");
                ViewData["MaLop"] = new SelectList(_context.LopHoc, "MaLop", "MaLop");

                // Lấy danh sách mã khoa từ cơ sở dữ liệu
                var khoaList = _context.Khoa
                                      .Select(k => new SelectListItem
                                      {
                                          Value = k.MaKhoa.ToString(),
                                          Text = k.TenKhoa
                                      })
                                      .ToList();

                // Truyền danh sách mã khoa vào ViewBag
                ViewBag.KhoaList = khoaList;

                return View(sinhVien);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("MaSV,HoTen,NgaySinh,DienThoai,Email,MaLop,MaChucVu")] SinhVien updatedSinhVien, IFormFile newImage, string maKhoa)
        {
            var currentUserId = User.Identity.Name.Split('@')[0];
            var sinhVien = await GetSinhVienAsync(currentUserId);

            if (sinhVien == null)
            {
                // Kiểm tra xem mã lớp có tồn tại không
                var existingLop = _context.LopHoc.FirstOrDefault(l => l.MaLop == updatedSinhVien.MaLop);

                if (existingLop == null)
                {
                    // Nếu mã lớp không tồn tại, thêm mới vào cơ sở dữ liệu với mã khoa tương ứng
                    var newLop = new LopHoc
                    {
                        MaLop = updatedSinhVien.MaLop.ToUpper(),
                        TenLop = updatedSinhVien.MaLop,
                        Khoahoc = "",
                        MaKhoa = maKhoa // Sử dụng mã khoa được truyền từ form
                    };

                    _context.LopHoc.Add(newLop);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Nếu mã lớp đã tồn tại, chỉ cập nhật lại mã khoa
                    existingLop.MaKhoa = maKhoa; // Sử dụng mã khoa được truyền từ form
                    _context.LopHoc.Update(existingLop);
                    await _context.SaveChangesAsync();
                }

                await CreateNewSinhVienAsync(updatedSinhVien, newImage, currentUserId);
            }
            else
            {
                await UpdateExistingSinhVienAsync(sinhVien, updatedSinhVien, newImage);
            }

            return RedirectToAction("Index", "Home");
        }


        private async Task<SinhVien> GetSinhVienAsync(string maSV)
        {
            return await _context.SinhVien.FindAsync(maSV);
        }

        private async Task CreateNewSinhVienAsync(SinhVien updatedSinhVien, IFormFile newImage, string currentUserId)
        {
            var sinhVien = new SinhVien
            {
                MaSV = currentUserId,
                HoTen = updatedSinhVien.HoTen,
                NgaySinh = updatedSinhVien.NgaySinh,
                DienThoai = updatedSinhVien.DienThoai,
                Email = User.Identity.Name,
                MaLop = updatedSinhVien.MaLop,
                MaChucVu = "CV04"
            };

            await SaveImageAndUpdateSinhVienAsync(sinhVien, newImage);
            await CreateOrUpdateQRCodeImageAsync(sinhVien);
        }

        private async Task UpdateExistingSinhVienAsync(SinhVien sinhVien, SinhVien updatedSinhVien, IFormFile newImage)
        {
            sinhVien.HoTen = sinhVien.HoTen;
            sinhVien.NgaySinh = sinhVien.NgaySinh;
            sinhVien.DienThoai = sinhVien.DienThoai;
            sinhVien.Email = sinhVien.Email;
            sinhVien.MaLop = sinhVien.MaLop;
            sinhVien.MaChucVu = sinhVien.MaChucVu;
            // Nếu sinh viên đã có ảnh cũ, xóa ảnh cũ trước khi cập nhật ảnh mới
            if (!string.IsNullOrEmpty(sinhVien.HinhAnh))
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "userimages", sinhVien.HinhAnh);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            await SaveImageAndUpdateSinhVienAsync(sinhVien, newImage);
            await CreateOrUpdateQRCodeImageAsync(sinhVien);

        }

        private async Task SaveImageAndUpdateSinhVienAsync(SinhVien sinhVien, IFormFile newImage)
        {
            if (newImage != null && newImage.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newImage.FileName);
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "userimages", fileName);
                using (var stream = new FileStream(uploadPath, FileMode.Create))
                {
                    await newImage.CopyToAsync(stream);
                }

                sinhVien.HinhAnh = fileName;
            }

            try
            {
                if (_context.Entry(sinhVien).State == EntityState.Detached)
                {
                    _context.Add(sinhVien);
                }
                else
                {
                    _context.Update(sinhVien);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SinhVienExists(sinhVien.MaSV))
                {
                    throw new NotFoundException(); // Bổ sung custom exception hoặc xử lý khác tùy nhu cầu
                }
                else
                {
                    throw; // Hoặc xử lý lỗi khác
                }
            }
        }







        [Authorize(Roles = "Administrators")]
        // Phương thức xuất Excel
        public IActionResult ExportToExcel()
        {
            // Lấy searchString từ Session
            var searchString = HttpContext.Session.GetString("searchString");

            var sinhViens = from s in _context.SinhVien.Include(s => s.ChucVu).Include(s => s.LopHoc)
                            select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                sinhViens = sinhViens.Where(s => s.MaSV.Contains(searchString)
                                                || s.HoTen.Contains(searchString)
                                                || s.ChucVu.TenChucVu.Contains(searchString)
                                                || s.LopHoc.MaLop.Contains(searchString));
            }
            // Tạo một package Excel mới
            using (var package = new ExcelPackage())
            {
                // Tạo một sheet mới
                var worksheet = package.Workbook.Worksheets.Add("Danh sách sinh viên");

                // Đặt tiêu đề cho các cột
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Mã Sinh Viên";
                worksheet.Cells[1, 3].Value = "Họ Tên";
                worksheet.Cells[1, 4].Value = "Ngày Sinh";
                worksheet.Cells[1, 5].Value = "Điện Thoại";
                worksheet.Cells[1, 6].Value = "Email";
                worksheet.Cells[1, 7].Value = "Mã Lớp";
                worksheet.Cells[1, 8].Value = "Chức Vụ";

                // Đặt dữ liệu cho các ô
                int row = 2;
                int i = 1;
                foreach (var sinhVien in sinhViens)
                {
                    worksheet.Cells[row, 1].Value = i;
                    worksheet.Cells[row, 2].Value = sinhVien.MaSV;
                    worksheet.Cells[row, 3].Value = sinhVien.HoTen;
                    worksheet.Cells[row, 4].Value = sinhVien.NgaySinh.ToShortDateString();
                    worksheet.Cells[row, 5].Value = sinhVien.DienThoai;
                    worksheet.Cells[row, 6].Value = sinhVien.Email;
                    worksheet.Cells[row, 7].Value = sinhVien.LopHoc.MaLop;
                    worksheet.Cells[row, 8].Value = sinhVien.ChucVu.TenChucVu;
                    row++;
                    i++;
                }

                // Tạo một stream để lưu trữ tệp Excel
                MemoryStream stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                HttpContext.Session.Remove("searchString");
                // Định dạng tên file với ngày tháng năm và thuộc tính được tìm kiếm
                if(searchString != null)
                {
                    string fileName = $"DanhSachSinhVien_{searchString.ToUpper()}_{DateTime.Today:ddMMyyyy}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else
                {
                    string fileName = $"DanhSachSinhVien_{DateTime.Today:ddMMyyyy}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

                }
                // Trả về tệp Excel như một file
            }
        }




        // GET: SinhViens
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var sinhViens = from s in _context.SinhVien.Include(s => s.ChucVu).Include(s => s.LopHoc)
                            select s;

            if (!string.IsNullOrEmpty(searchString))
            {
                sinhViens = sinhViens.Where(s => s.MaSV.Contains(searchString)
                                                || s.HoTen.Contains(searchString)
                                                || s.ChucVu.TenChucVu.Contains(searchString)
                                                || s.LopHoc.MaLop.Contains(searchString));

                // Lưu searchString vào Session
                HttpContext.Session.SetString("searchString", searchString);
            }

            int pageSize = 10; // Số lượng mục trên mỗi trang
            return View(await PaginatedList<SinhVien>.CreateAsync(sinhViens.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [Authorize]
        public async Task<IActionResult> BanChuNhiem()
        {
            var applicationDbContext = _context.SinhVien.Include(s => s.ChucVu).Include(s => s.LopHoc).Where(s => s.ChucVu.MaChucVu != "CV04" && s.ChucVu.MaChucVu != null);
            return View(await applicationDbContext.ToListAsync());
        }


        // GET: SinhViens/Details/5

/*        [Authorize]*/
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.SinhVien == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien
                .Include(s => s.ChucVu)
                .Include(s => s.LopHoc)
                .FirstOrDefaultAsync(m => m.MaSV == id);
            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }

        // GET: SinhViens/Create
        [Authorize(Roles = "Administrators")]
        public IActionResult Create()
        {
            ViewData["MaChucVu"] = new SelectList(_context.ChucVu, "MaChucVu", "TenChucVu");
            ViewData["MaLop"] = new SelectList(_context.LopHoc, "MaLop", "MaLop");
            return View();
        }

        // POST: SinhViens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Create([Bind("MaSV,HoTen,NgaySinh,DienThoai,Email,MaLop,MaChucVu")] SinhVien sinhVien, IFormFile HinhAnh)
        {
/*            if (ModelState.IsValid)
            {*/
                if (HinhAnh != null && HinhAnh.Length > 0)
                {
                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);

                    // Lưu file vào vị trí chỉ định (thay thế bằng logic của bạn)
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "userimages", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnh.CopyToAsync(stream);
                    }

                    // Cập nhật tên file cho model (tùy chọn)
                    sinhVien.HinhAnh = fileName;
                }

                _context.Add(sinhVien);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            /*}*/

            ViewData["MaChucVu"] = new SelectList(_context.ChucVu, "MaChucVu", "TenChucVu", sinhVien.MaChucVu);
            ViewData["MaLop"] = new SelectList(_context.LopHoc, "MaLop", "TenLop", sinhVien.MaLop);
            return View(sinhVien);
        }



        // GET: SinhViens/Edit/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien.FindAsync(id);
            if (sinhVien == null)
            {
                return NotFound();
            }

            ViewData["MaLop"] = new SelectList(_context.LopHoc, "MaLop", "MaLop", sinhVien.MaLop);
            ViewData["MaChucVu"] = new SelectList(_context.ChucVu, "MaChucVu", "TenChucVu", sinhVien.MaChucVu);
            return View(sinhVien);
        }

        // POST: SinhViens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: SinhViens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("MaSV,HoTen,NgaySinh,DienThoai,Email,MaLop,MaChucVu")] SinhVien sinhVien, IFormFile HinhAnh)
        {
            if (id != sinhVien.MaSV)
            {
                return NotFound();
            }

            // Lấy SinhVien cũ từ cơ sở dữ liệu
            var sinhVienOld = await _context.SinhVien.AsNoTracking().FirstOrDefaultAsync(s => s.MaSV == id);
            if (sinhVienOld == null)
            {
                return NotFound();
            }


                // Kiểm tra nếu có hình ảnh mới được tải lên
                if (HinhAnh != null && HinhAnh.Length > 0)
                {
                    // Nếu sinh viên đã có ảnh cũ, xóa ảnh cũ trước khi cập nhật ảnh mới
                    if (!string.IsNullOrEmpty(sinhVienOld.HinhAnh))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "userimages", sinhVienOld.HinhAnh);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "userimages", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await HinhAnh.CopyToAsync(stream);
                    }
                    sinhVien.HinhAnh = fileName; // Cập nhật với tên file hình ảnh mới
                }
                else
                {
                    sinhVien.HinhAnh = sinhVienOld.HinhAnh; // Giữ nguyên hình ảnh cũ nếu không có hình mới
                }

                try
                {
                    _context.Update(sinhVien);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SinhVienExists(sinhVien.MaSV))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            
            // Load lại thông tin nếu ModelState không hợp lệ
            ViewData["MaChucVu"] = new SelectList(_context.ChucVu, "MaChucVu", "MaChucVu", sinhVien.MaChucVu);
            ViewData["MaLop"] = new SelectList(_context.LopHoc, "MaLop", "MaLop", sinhVien.MaLop);
            return View(sinhVien);
        }


        // GET: SinhViens/Delete/5
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sinhVien = await _context.SinhVien.FindAsync(id);
            if (sinhVien == null)
            {
                return NotFound();
            }

            return View(sinhVien);
        }


        // POST: SinhViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sinhVien = await _context.SinhVien.FindAsync(id);

            // Xử lý xóa file ảnh (nếu có)

            if (sinhVien.HinhAnh != null)
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "userimages", sinhVien.HinhAnh);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
             if (sinhVien.DuongdanQR != null)
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "qrcode", sinhVien.DuongdanQR);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.SinhVien.Remove(sinhVien);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SinhVienExists(string id)
        {
          return (_context.SinhVien?.Any(e => e.MaSV == id)).GetValueOrDefault();
        }
    }
}
