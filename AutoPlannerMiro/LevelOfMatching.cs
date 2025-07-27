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

        public void IncrementTries()
        {
            TotalTries++;
        }

        public void UpdateSevernity(int newLevel)
        {
            LevelOfSevernity = newLevel;
        }

        // Przykładowa metoda oceny
        public void EvaluateMatching(/* params możesz dodać listę orderów, statusy itd. */)
        {
            // Tutaj zrób logikę oceny dopasowania, np. na podstawie OrdersMatched, OrdersUnmatched
            // i ustaw LevelOfSevernity odpowiednio (1/2/3)
        }
    }

}
