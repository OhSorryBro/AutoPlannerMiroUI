using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public static class Terminal
    {
        public static event Action<string> OnWriteLine;

        public static void WriteLine(string message)
        {
            OnWriteLine?.Invoke(message);
        }
    }
}
