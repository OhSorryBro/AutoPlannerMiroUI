using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public class CreatorFormerenStation
    // Creates a list of formeren stations based on the user input. Every day in operation is slightly different,
    // so it is crucial to be able to create a list of stations dynamically.
    {
        public static List<FormerenStation> CreatorFormerenStations(int amount)
        {
            var stations = new List<FormerenStation>();
            for (int i = 1; i <= amount; i++)
            {
                stations.Add(new FormerenStation { FormerenStationID = i, TimeAvailableFormeren = 1440, x = i * 100, y = 0 });
            }
            return stations;
        }
    }

}
