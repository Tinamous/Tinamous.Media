using System;

namespace AnalysisUK.Tinamous.Media.Domain.Helpers
{
    public class SystemDate
    {
        private static Func<DateTime> _utcNowFunc = () => DateTime.UtcNow;

        public static DateTime UtcNow
        {
            get
            {
                return _utcNowFunc.Invoke();
            }
        }

        public static void Reset()
        {
            _utcNowFunc = () => DateTime.UtcNow;
        }

        public static void Set(DateTime dateTime)
        {
            _utcNowFunc = () => dateTime;
        } 
    }
}