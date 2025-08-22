using AutoPlannerMiroUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace AutoPlannerMiro
{
    public partial class Form1 : Form, IWizardView
    {
        private WizardPresenter _presenter;

        public Form1()
        {
            InitializeComponent();
            categoryNames = (RunPlannerAsync.Categories ?? new())
                                   .Select(c => c.Category)
                                   .ToList();

            this.Icon = new Icon("Automatic-planner-Miro-Icon.ico");
            this.Load += (s, e) => textBoxInput.Focus();

            var categoryNames = (RunPlannerAsync.Categories ?? new())
                .Select(c => c.Category)
                .ToList();

            var questions = new[]
            {
            "How many Forming Stations?",
            "How many Ready Locations?",
            "Choose scenario (1, 2, 3):",
            "Choose layout: H (standard) or K (alternative):",
        };

            var state = new WizardState(questions, categoryNames);
            var runner = new PlannerRunner();
            _presenter = new WizardPresenter(this, state, runner);

            Terminal.OnWriteLine += msg => WriteLine(msg);
            _presenter.Start();
        }

        // IWizardView
        public void WriteLine(string line) => AddLineToTerminal(line);
        public void DisableInput() => textBoxInput.Enabled = false;
        public void EnableInput() => textBoxInput.Enabled = true;
        public void FocusInput() => textBoxInput.Focus();

        private void AddLineToTerminal(string line)
        {
            if (textBoxTerminal.Text.Length > 0)
                textBoxTerminal.AppendText(Environment.NewLine);
            textBoxTerminal.AppendText(line);
            textBoxTerminal.SelectionStart = textBoxTerminal.Text.Length;
            textBoxTerminal.ScrollToCaret();
        }

        private async void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string input = textBoxInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            WriteLine("> " + input);
            textBoxInput.Clear();

            await _presenter.OnInputAsync(input);
        }
    }
}