using System;
using System.Diagnostics;
using System.IO;

namespace AutoPlannerMiro
{
    internal static class DebugHarness
    {
        [Conditional("DEBUG")]
        public static void RunFileHelpers()
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
