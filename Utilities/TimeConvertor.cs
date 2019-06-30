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
    }
}
