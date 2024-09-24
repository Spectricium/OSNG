using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSNGLib
{
    public class BetterTimeSpan
    {
        public static string GetBetterTimeSpanString(TimeSpan ts)
        {
            StringBuilder result = new StringBuilder();

            long totalNanoseconds = ts.Ticks * 100;
            long totalMicroseconds = ts.Ticks / ( TimeSpan.TicksPerMillisecond / 1000 );
            long totalMilliseconds = ts.Ticks / TimeSpan.TicksPerMillisecond;
            long totalSeconds = (long)ts.TotalSeconds;
            long totalMinutes = (long)ts.TotalMinutes;
            long totalHours = (long)ts.TotalHours;
            long totalDays = (long)ts.TotalDays;

            if (totalDays > 0)
                result.AppendFormat("{0}d ", totalDays);
            if (totalHours % 24 > 0)
                result.AppendFormat("{0}h ", totalHours % 24);
            if (totalMinutes % 60 > 0)
                result.AppendFormat("{0}m ", totalMinutes % 60);
            if (totalSeconds % 60 > 0)
                result.AppendFormat("{0}s ", totalSeconds % 60);
            if (totalMilliseconds % 1000 > 0)
                result.AppendFormat("{0}ms ", totalMilliseconds % 1000);
            if (totalMicroseconds % 1000 > 0)
                result.AppendFormat("{0}µs ", totalMicroseconds % 1000); // µ es el símbolo de micro
            if (totalNanoseconds % 1000 > 0)
                result.AppendFormat("{0}ns", totalNanoseconds % 1000);
            return result.ToString().Trim();
        }
    }
}
