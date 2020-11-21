using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SYSTEMTIME
    {
        private ushort year;
        private short month;
        private short dayOfWeek;
        private short day;
        private short hour;
        private short minute;
        private short second;
        private short millisecond;

        public static SYSTEMTIME FromByteArray(byte[] array, int offset)
        {
            SYSTEMTIME st = new SYSTEMTIME
            {
                year = BitConverter.ToUInt16(array, offset),
                month = BitConverter.ToInt16(array, offset + 2),
                day = BitConverter.ToInt16(array, offset + 6),
                hour = BitConverter.ToInt16(array, offset + 8),
                minute = BitConverter.ToInt16(array, offset + 10),
                second = BitConverter.ToInt16(array, offset + 12)
            };

            return st;
        }
        public static SYSTEMTIME FromDateTime(DateTime dt)
        {
            SYSTEMTIME st = new SYSTEMTIME
            {
                year = (ushort)dt.Year,
                month = (short)dt.Month,
                dayOfWeek = (short)dt.DayOfWeek,
                day = (short)dt.Day,
                hour = (short)dt.Hour,
                minute = (short)dt.Minute,
                second = (short)dt.Second,
                millisecond = (short)dt.Millisecond
            };

            return st;
        }

        public DateTime ToDateTime(DateTimeKind kind)
        {
            if (year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0)
            {
                return DateTime.MinValue;
            }
            return new DateTime(year, month, day, hour, minute, second, millisecond, kind);
        }
    }
}