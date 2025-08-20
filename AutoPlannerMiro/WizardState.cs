using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPlannerMiroUI
{
    public class WizardState
    {
        public int Step { get; private set; } = 0;
        public List<string> Answers { get; } = new();
        public int CategoryStep { get; private set; } = 0;
        public List<int> CategoryCounts { get; } = new();
        public IReadOnlyList<string> Questions { get; }
        public IReadOnlyList<string> CategoryNames { get; }

        public WizardState(IReadOnlyList<string> questions, IReadOnlyList<string> categoryNames)
        {
            Questions = questions ?? throw new ArgumentNullException(nameof(questions));
            CategoryNames = categoryNames ?? Array.Empty<string>();
        }

        public string CurrentQuestion =>
            Step < Questions.Count ? Questions[Step] :
            CategoryStep < CategoryNames.Count ? $"How many slots for {CategoryNames[CategoryStep]}?" :
            null;

        public bool IsWizardQuestionsPhase => Step < Questions.Count;
        public bool IsCategoriesPhase => Step >= Questions.Count && CategoryStep < CategoryNames.Count;
        public bool IsCompleted => Step >= Questions.Count && CategoryStep >= CategoryNames.Count;

        public (bool ok, string error) AcceptInput(string input)
        {
            if (IsWizardQuestionsPhase)
            {
                switch (Step)
                {
                    case 0:
                        if (!InputValidator.TryParseIntInRange(input, 1, 5, out _))
                            return (false, "Please enter a whole number between 1 and 5 (forming stations).");
                        Answers.Add(input); Step++; return (true, null);

                    case 1:
                        if (!InputValidator.TryParseIntInRange(input, 1, 16, out _))
                            return (false, "Please enter a whole number between 1 and 16 (ready locations).");
                        Answers.Add(input); Step++; return (true, null);

                    case 2:
                        if (!InputValidator.TryParseIntInRange(input, 1, 3, out _))
                            return (false, "Scenario must be 1, 2, or 3.");
                        Answers.Add(input); Step++; return (true, null);

                    case 3:
                        if (!InputValidator.TryParseLayoutHK(input, out var layout))
                            return (false, "Layout must be 'H' or 'K'.");
                        Answers.Add(layout); Step++; return (true, null);
                }
            }
            else if (IsCategoriesPhase)
            {
                if (!InputValidator.TryParseNonNegativeInt(input, out int slots))
                    return (false, "Please enter a non-negative whole number (0 or more).");

                CategoryCounts.Add(slots);
                CategoryStep++;
                return (true, null);
            }

            return (false, "No more input expected.");
        }
    }
}
