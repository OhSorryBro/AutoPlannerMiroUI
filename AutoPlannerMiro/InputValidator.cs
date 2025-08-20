using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPlannerMiroUI
{
    public static class InputValidator
    {
        public static bool TryParseIntInRange(string s, int min, int max, out int value)
        {
            if (int.TryParse(s, out value))
                return value >= min && value <= max;
            value = 0;
            return false;
        }

        public static bool TryParseNonNegativeInt(string s, out int value)
        {
            if (int.TryParse(s, out value))
                return value >= 0;
            value = 0;
            return false;
        }

        public static bool TryParseLayoutHK(string s, out string layout)
        {
            layout = (s ?? "").Trim().ToUpperInvariant();
            return layout == "H" || layout == "K";
        }
    }
}
