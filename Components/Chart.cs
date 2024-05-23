using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using website_CLB_HTSV.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace website_CLB_HTSV.Components
{
    public class Chart : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public Chart(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime vietnamTimeNow = TimeZoneHelper.GetVietNamTime(utcNow);
            DateTime vietnamTimeCutOff = vietnamTimeNow.AddMonths(-11);

            var activities = await _context.HoatDong
                .Where(h => h.ThoiGian >= vietnamTimeCutOff)
                .ToListAsync();

            // Khởi tạo dictionary để lưu trữ số lượng đăng ký theo tháng
            var registrationData = new Dictionary<int, int>();
            // Khởi tạo dictionary để lưu trữ số lượng tham gia theo tháng
            var participationData = new Dictionary<int, int>();

            // Tính toán dữ liệu đăng ký và tham gia theo tháng
            foreach (var activity in activities)
            {
                var month = activity.ThoiGian.Month;

                // Lấy số lượng đăng ký hoạt động
                var registrationCount = await _context.DangKyHoatDong
                    .Where(dk => dk.MaHoatDong == activity.MaHoatDong)
                    .CountAsync();

                if (registrationData.ContainsKey(month))
                {
                    registrationData[month] += registrationCount;
                }
                else
                {
                    registrationData.Add(month, registrationCount);
                }

                // Lấy số lượng sinh viên tham gia hoạt động
                var studentCount = await _context.ThamGiaHoatDong
                    .Where(shd => shd.MaHoatDong == activity.MaHoatDong)
                    .CountAsync();

                if (participationData.ContainsKey(month))
                {
                    participationData[month] += studentCount;
                }
                else
                {
                    participationData.Add(month, studentCount);
                }
            }

            // Tạo mảng chứa tên các tháng bằng tiếng Việt
            var vietnameseCulture = new CultureInfo("vi-VN");
            // Tạo mảng labels và data cho biểu đồ
            var labels = new string[12];
            var registrationCounts = new int[12];
            var participationCounts = new int[12];
            var currentMonth = TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).Month;
            for (int i = 0; i < 12; i++)
            {
                var month = currentMonth - i;
                if (month <= 0)
                {
                    month += 12;
                }
                labels[i] = vietnameseCulture.DateTimeFormat.GetMonthName(month);
                if (registrationData.ContainsKey(month))
                {
                    registrationCounts[i] = registrationData[month];
                }
                else
                {
                    registrationCounts[i] = 0;
                }

                if (participationData.ContainsKey(month))
                {
                    participationCounts[i] = participationData[month];
                }
                else
                {
                    participationCounts[i] = 0;
                }
            }

            ViewBag.MonthlyParticipationLabels = labels;
            ViewBag.MonthlyRegistrationCounts = registrationCounts;
            ViewBag.MonthlyParticipationCounts = participationCounts;

            return View("Index");
        }
    }
}
