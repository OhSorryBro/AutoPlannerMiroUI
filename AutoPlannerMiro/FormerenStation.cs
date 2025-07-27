using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public class FormerenStation
    // Define a class to represent a Formeren station with an ID and available time.
    // This class will be used to create a list of Formeren stations.
    // This class contains properties for the station ID and the time available for formeren.
    // Stack is beeing used to store orders added to the station, which can be later used for putting to RDY station.
    {
        public int FormerenStationID;
        public int TimeAvailableFormeren;
        public int x;
        public int y;
        public int orderStart;
        public int orderEnd;
        public HashSet<int> TimeBusy { get; set; } = new HashSet<int>();
        public Stack<(string OrderCategory, int x, int y, string Color, int orderStart, int orderEnd)> OrdersAdded { get; set; }
            = new Stack<(string, int, int, string, int orderStart, int orderEnd)>();

    }
}
