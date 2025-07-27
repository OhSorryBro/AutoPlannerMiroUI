using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public class ReadyLocation
    // Define a class to represent a Formeren Ready Location with an ID (operation see it as a dock) and available time.
    // This class will be used to create a list of Formeren Ready locations.
    // This class contains properties for the location ID and the time available when order can be awaiting for pick-up.
    // Stack is beeing used to store orders added to the Location, gets populated by FormerenStation.
    {
        public int ReadyLocationID;
        public int TimeAvailableReady;
        public int x;
        public int y;
        public int orderStart;
        public int orderEnd;
        public HashSet<int> TimeBusy { get; set; } = new HashSet<int>();
        public Stack<(string OrderCategory, int x, int y, string Color, int orderStart, int orderEnd)> OrdersAdded { get; set; }
    = new Stack<(string, int, int, string, int orderStart, int orderEnd)>();
    }
}
