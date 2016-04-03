using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ostats
{

    public static class Extensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> seq)
        {
            return seq ?? Enumerable.Empty<T>();
        }

        public static TimeSpan? ToTimeSpan(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var parts = s.Split(':');
            if (parts.Length != 2) return null;

            var minutes = parts[0].ToInt32();
            var seconds = parts[1].ToInt32();

            if (minutes == null || seconds == null) return null;

            return new TimeSpan(0, minutes.Value, seconds.Value);
        }
        public static int? ToInt32(this string s)  
        {
            int i;
            if (int.TryParse(s, out i))
                return i;
            return null;
        }
    }
}
