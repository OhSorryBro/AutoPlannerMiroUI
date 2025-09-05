using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPlannerMiro;

namespace AutoPlannerMiro
{
    public class LevelOfMatching
    {
        public Dictionary<int, double[]> ScenarioWeights { get; set; }
        public int ScenarioUsed { get; set; }
        public int TotalTries { get; set; }
        public int LevelOfSevernity { get; set; }
        public int OrdersMatched { get; set; }
        public int OrdersUnmatched { get; set; }

        public LevelOfMatching(int scenario)
        {
            ScenarioWeights = new Dictionary<int, double[]>
        {
            { 1, new double[] { 0.8, 0.15, 0.05 } },   // Scenario 1: 80%/15%/5%
            { 2, new double[] { 0.55, 0.30, 0.15 } },  // Scenario 2: 55%/30%/15%
            { 3, new double[] { 0.3, 0.35, 0.35 } }    // Scenario 3: 33%/33%/34%
        };
            ScenarioUsed = scenario;
            TotalTries = 0;
            LevelOfSevernity = 1;
            OrdersMatched = 0;
            OrdersUnmatched = 0;
        }


    public static int GetLoadingTimeBySeverity(int levelOfSevernity)
    {
        switch (levelOfSevernity)
        {
            case 1:
                return 90;
            case 2:
                return 90;
            case 3:
                return 80;
            case 4:
                return 70;
            default:
                return 90;
        }
    }

        public void IncrementTries()
        {
            TotalTries++;
        }

        public void UpdateSevernity(int newLevel)
        {
            LevelOfSevernity = newLevel;
        }

        // MIKE TO_DO
        public void EvaluateMatching()
        {
            // Some logic to evaluate matching? OrdersMatched, OrdersUnmatched

        }
    }

}
