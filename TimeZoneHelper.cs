namespace website_CLB_HTSV
{
    public class TimeZoneHelper
    {
        private static readonly TimeZoneInfo VietNamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public static DateTime GetVietNamTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietNamTimeZone);
        }
    }
}
