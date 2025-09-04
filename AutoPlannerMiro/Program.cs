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
            RunFileHelpersTest();




            //Testing area finishes here
            return; 
#endif
            // Release → UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void RunFileHelpersTest()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "priority_orders.csv");
            try
            {
                var orders = FileHelpers.ReadPriorityOrders(path);
                Console.WriteLine($"Found {orders.Count} records in: {path}");
                foreach (var kvp in orders)
                    Console.WriteLine($"{kvp.Key} => ({kvp.Value.Item1}, {kvp.Value.Item2})");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Read error: " + ex.Message);
            }
            Console.WriteLine("Use Enter to finish...");
            Console.ReadLine();
        }
    }
}
