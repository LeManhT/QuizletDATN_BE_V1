namespace Quizlet_App_Server.Utility
{
    public class TimeHelper
    {
        public static DateTime beginEpoch = new DateTime(1970, 1, 1, 0, 0, 0);
        public static long UnixTimeNow => ToUnixTime(DateTime.Now);
        public static long ToUnixTime(DateTime dateTime)
        {
            var timeSpan = (dateTime - beginEpoch);
            return (long)timeSpan.TotalSeconds;
        }
        public static DateTime ToDateTime(long unix)
        {
            return beginEpoch.AddSeconds(unix);
        }
    }
}
