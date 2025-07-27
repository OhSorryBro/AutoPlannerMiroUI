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
                return dict;

            var lines = File.ReadAllLines(path).Skip(1); // pomiń nagłówek
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
    }
}
