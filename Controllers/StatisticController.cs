using Microsoft.AspNetCore.Mvc;
using website_CLB_HTSV.Data;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace website_CLB_HTSV.Controllers
{
    public class StatisticController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime vietnamTimeNow = TimeZoneHelper.GetVietNamTime(utcNow);
            DateTime vietnamTimeCutOff = vietnamTimeNow.AddMonths(-11);

            var activities = _context.HoatDong
                .Where(h => h.ThoiGian >= vietnamTimeCutOff)
                .ToList();

            // Khởi tạo dictionary để lưu trữ số lượng tham gia theo tháng
            var participationData = new Dictionary<int, int>();

            // Tính toán dữ liệu tham gia theo tháng
            foreach (var activity in activities)
            {
                var month = activity.ThoiGian.Month;
                if (participationData.ContainsKey(month))
                {
                    participationData[month]++;
                }
                else
                {
                    participationData.Add(month, 1);
                }
            }

            // Tạo mảng chứa tên các tháng bằng tiếng Việt
            var vietnameseCulture = new CultureInfo("vi-VN");
            // Tạo mảng labels và data cho biểu đồ
            var labels = new string[12];
            var participatedCounts = new int[12];
            var currentMonth = TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).Month;
            for (int i = 0; i < 12; i++)
            {
                var month = currentMonth - i;
                if (month <= 0)
                {
                    month += 12;
                }
                labels[i] = vietnameseCulture.DateTimeFormat.GetMonthName(month);
                if (participationData.ContainsKey(month))
                {
                    participatedCounts[i] = participationData[month];
                }
                else
                {
                    participatedCounts[i] = 0;
                }
            }

            ViewBag.MonthlyParticipationLabels = labels;
            ViewBag.MonthlyParticipationCounts = participatedCounts;

            return View();
        }
    }
}
