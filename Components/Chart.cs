using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Manage_CLB_HTSV.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Manage_CLB_HTSV.Components
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

            var registrationData = new Dictionary<int, int>();
            var participationData = new Dictionary<int, int>();

            foreach (var activity in activities)
            {
                var month = activity.ThoiGian.Month;

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

            var vietnameseCulture = new CultureInfo("vi-VN");
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

            ViewBag.MonthlyParticipationLabels = labels.Reverse().ToArray();
            ViewBag.MonthlyRegistrationCounts = registrationCounts.Reverse().ToArray();
            ViewBag.MonthlyParticipationCounts = participationCounts.Reverse().ToArray();

            return View("Index");
        }
    }
}
