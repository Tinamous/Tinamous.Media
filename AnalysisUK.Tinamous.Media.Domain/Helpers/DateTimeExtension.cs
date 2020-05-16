using System;

namespace AnalysisUK.Tinamous.Media.Domain.Helpers
{
    public static class DateTimeExtension
    {
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static decimal ToUnixSeconds(this DateTime dateTime)
        {
            return Convert.ToDecimal(dateTime.Subtract(EpochDateTime).TotalSeconds);
        }

        public static long ToLongUnixSeconds(this DateTime dateTime)
        {
            return Convert.ToInt64(dateTime.Subtract(EpochDateTime).TotalSeconds);
        }
    }
}