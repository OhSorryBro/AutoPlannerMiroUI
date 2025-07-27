using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public class CreatorReadyLocation
    // Creates a list of ready locations based on the user input. Every day in operation is slightly different,
    // so it is crucial to be able to create a list of locations dynamically.
    {
        public static List<ReadyLocation> CreatorReadyLocations(int amount)
        {
            var locations = new List<ReadyLocation>();
            for (int i = 1; i <= amount; i++)
            {
                locations.Add(new ReadyLocation { ReadyLocationID = i, TimeAvailableReady = 1440, x = i * 100, y = 0 });
            }
            return locations;
        }
    }

}
