using System;

namespace Pegasus_backend.Utilities
{
    public static class TimeConvertor
    {
        public static DateTime ToNZTimezone(this DateTime utc)
        {
            DateTime nzTime = new DateTime();
            try
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, nztZone);
                return nzTime;
            }
            catch (TimeZoneNotFoundException)
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, nztZone);
                return nzTime;
            }
            catch (InvalidTimeZoneException)
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
                nzTime = TimeZoneInfo.ConvertTimeFromUtc(utc, nztZone);
                return nzTime;
            }
        }

        public static int ToDayOfWeek(this DateTime inputTime)
        {
            int dayOfWeek = inputTime.DayOfWeek == 0 ? 7 : (int)inputTime.DayOfWeek;
            return dayOfWeek;
        }

        public static string getDayOfWeek(int value)
        {
            switch (value)
            {
                case 1: return "Monday";
                case 2: return "Tuesday";
                case 3: return "Wednesday";
                case 4: return "Thursday";
                case 5: return "Friday";
                case 6: return "Saturday";
                case 7: return "Sunday";
                default: return ("Day of week not found");
            }
        }
    }
}
