using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoPlannerMiro
{
    internal static class Program
    {
#if DEBUG
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
#endif

        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            ///// ADD here what need to be tested
            AllocConsole();
            DebugHarness.RunFileHelpers();




            //Testing area finishes here
            return; 
#endif
            // Release → UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
