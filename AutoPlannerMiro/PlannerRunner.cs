using AutoPlannerMiro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPlannerMiroUI
{
    public class PlannerRunner
    {
        public async Task RunAsync(WizardState state, CancellationToken ct = default)
        {
            // tu można bezpiecznie parsować, bo walidacja już była
            int formerenStationAmount = int.Parse(state.Answers[0]);
            int readyLocationAmount = int.Parse(state.Answers[1]);
            int scenario = int.Parse(state.Answers[2]);
            string layoutInput = state.Answers[3];

            var planner = new RunPlannerAsync();
            await planner.Run(
                formerenStationAmount,
                readyLocationAmount,
                scenario,
                layoutInput,
                state.CategoryCounts
            );
        }
    }
}
