using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public class CategoryCount
    // Used to create a list of categories with information how many order slots are requested by the user.
    {
        public string Category { get; set; }
        public int Duration { get; set; }
        public int Count { get; set; }
        public int DurationGroup { get; set; }
        // 1 = shortest time needed, 3 = longest time needed.
        public string Color { get; set; }
    }

}
