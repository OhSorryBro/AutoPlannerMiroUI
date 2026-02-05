using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPlannerMiroUI
{
    public readonly record struct TimeRange(int Start, int End)
    {
        public int Duration => End - Start + 1;
        public static TimeRange FromStartAndDuration (int start, int duration)
        {
            if (duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration), "duration must be > 0");
            return new TimeRange(start, start + duration - 1);
        }
        public TimeRange NextAfter(int duration)
        { return FromStartAndDuration(End + 1, duration); }
        public override string ToString()
        {
            return $"{Start}-{End}";
        }
    }
}
