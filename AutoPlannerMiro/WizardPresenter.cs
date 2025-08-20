using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPlannerMiroUI
{
    public interface IWizardView
    {
        void WriteLine(string text);
        void DisableInput();
        void EnableInput();
        void FocusInput();
    }

    public class WizardPresenter
    {
        private readonly IWizardView _view;
        private readonly WizardState _state;
        private readonly PlannerRunner _runner;

        public WizardPresenter(IWizardView view, WizardState state, PlannerRunner runner)
        {
            _view = view;
            _state = state;
            _runner = runner;
        }

        public void Start()
        {
            _view.WriteLine("Welcome to the Planner application");
            _view.WriteLine("This application is designed to help you plan your work - load effectively at the XXXXXXX department.");
            _view.WriteLine("It will assist you in determining the best order slots based on the available time at the Formeren station and Ready locations.");
            if (_state.CurrentQuestion != null)
                _view.WriteLine(_state.CurrentQuestion);
            _view.FocusInput();
        }

        public async Task OnInputAsync(string input)
        {
            var (ok, error) = _state.AcceptInput(input);
            if (!ok)
            {
                _view.WriteLine(error);
                return;
            }

            if (_state.IsCompleted)
            {
                _view.WriteLine("All category slot numbers collected! Running planner...");
                _view.DisableInput();
                try
                {
                    await _runner.RunAsync(_state);
                    _view.WriteLine("Planner finished.");
                }
                catch (Exception ex)
                {
                    _view.WriteLine("Planner failed: " + ex.Message);
                    _view.EnableInput();
                }
                return;
            }

            // kolejny prompt
            if (_state.IsWizardQuestionsPhase && _state.Step == _state.Questions.Count)
            {
                _view.WriteLine("Wizard completed! Now enter number of slots for each category:");
            }

            if (_state.CurrentQuestion != null)
                _view.WriteLine(_state.CurrentQuestion);
        }
    }
}
