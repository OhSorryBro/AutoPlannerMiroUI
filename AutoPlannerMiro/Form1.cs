using System;
using System.Windows.Forms;

namespace AutoPlannerMiro
{
    public partial class Form1 : Form
    {
        private int wizardStep = 0;
        private string[] wizardQuestions = new[]
        {
            "How many Forming Stations?",
            "How many Ready Locations?",
            "Choose scenario (1, 2, 3):",
            "Choose layout: H (standard) or K (alternative):",
            "Type any value to finish wizard..."
        };

        public Form1()
        {
            InitializeComponent();

            // Ustaw focus na textboxInput po starcie
            this.Load += (s, e) => textBoxInput.Focus();

            // Pierwsza linia terminala i pierwsze pytanie
            AddLineToTerminal("Welcome to AutoPlanner Miro!");
            AddLineToTerminal(wizardQuestions[0]);
        }

        private void AddLineToTerminal(string line)
        {
            if (textBoxTerminal.Text.Length > 0)
                textBoxTerminal.AppendText(Environment.NewLine);
            textBoxTerminal.AppendText(line);
            textBoxTerminal.SelectionStart = textBoxTerminal.Text.Length;
            textBoxTerminal.ScrollToCaret();
        }

        private void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string input = textBoxInput.Text.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    AddLineToTerminal("> " + input);
                    textBoxInput.Clear();

                    // Prosty wizard krok po kroku
                    wizardStep++;
                    if (wizardStep < wizardQuestions.Length)
                    {
                        AddLineToTerminal(wizardQuestions[wizardStep]);
                    }
                    else
                    {
                        AddLineToTerminal("Wizard completed! Here you can start planner logic...");
                        textBoxInput.Enabled = false;
                    }
                }
            }
        }
    }
}