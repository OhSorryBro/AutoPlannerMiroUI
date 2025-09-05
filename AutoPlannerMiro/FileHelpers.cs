using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public static class FileHelpers
    {
        public static Dictionary<int, int> ReadReadyLocationStatus(string path)
        {
            var dict = new Dictionary<int, int>();
            if (!File.Exists(path))
            {
                Terminal.WriteLine("ERROR: File with Ready Location status data is not found: " + path);
                return dict;
            }
            var lines = File.ReadAllLines(path).Skip(1); 
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                int id = int.Parse(parts[0]);
                string status = parts[1];
                int occupiedUntil = int.Parse(parts[2]);

                if (status == "Occupied" && occupiedUntil > 0)
                    dict[id] = occupiedUntil;
            }
            return dict;
        }

      public static Dictionary<string, List<(int idealMinute, int tolMin)>> ReadPriorityOrders(
    string path,
    int defaultToleranceMin = 60)
{
            var dict = new Dictionary<string, List<(int idealMinute, int tolMin)>>(StringComparer.OrdinalIgnoreCase);


            if (!File.Exists(path))
    {
        Terminal.WriteLine("ERROR: File with priority data is not found: " + path);
        return dict;
    }

    var lines = File.ReadAllLines(path).Skip(1); // pomijamy nagłówek
    foreach (var raw in lines)
    {
        if (string.IsNullOrWhiteSpace(raw)) continue;

        var parts = raw.Split(';');
        if (parts.Length < 3)
        {
            Terminal.WriteLine($"WARN: Bad row (expected 3 columns): '{raw}'");
            continue;
        }

        string orderId = parts[0].Trim();
        if (!int.TryParse(parts[1].Trim(), out int idealMinute))
        {
            Terminal.WriteLine($"WARN: Bad TimeWindow: '{parts[1]}'");
            continue;
        }
        if (!int.TryParse(parts[2].Trim(), out int amount) || amount <= 0)
        {
            Terminal.WriteLine($"WARN: Bad AmountOfOrders: '{parts[2]}'");
            continue;
        }

        if (!dict.TryGetValue(orderId, out var list))
        {
            list = new List<(int,int)>();
            dict[orderId] = list;
        }

        // Dodajemy „amount” kopii tego samego okna
        for (int i = 0; i < amount; i++)
            list.Add((idealMinute, defaultToleranceMin));
    }

    // posortuj okna w ramach każdej kategorii, dla przewidywalności
    foreach (var kv in dict)
        kv.Value.Sort((a, b) => a.idealMinute.CompareTo(b.idealMinute));

    return dict;
}
    }
}
