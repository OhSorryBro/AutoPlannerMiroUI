using System;
using System.ComponentModel.Design;
using System.Numerics;
using System.Windows.Forms;

namespace AutoPlannerMiro
{
    public partial class Form1 : Form
    {
        private int wizardStep = 0;
        private List<string> wizardAnswers = new List<string>();
        private List<int> categoryCounts = new List<int>();
        private string[] wizardQuestions = new[]
        {
        "How many Forming Stations?",
        "How many Ready Locations?",
        "Choose scenario (1, 2, 3):",
        "Choose layout: H (standard) or K (alternative):",
    };

        private int categoryInputStep = 0;
        private List<string> categoryNames = new List<string>
    {
        "Order_Type1", "Order_Type2", "Order_Type3", "Order_Type4", "Order_Type5",
        "Order_Type6", "Order_Type7", "Order_Type8", "Order_Type9", "Order_Type10",
        "Order_Type11", "Order_Type12", "Order_Type13"
    };

        public Form1()
        {
            InitializeComponent();
            this.Icon = new Icon("Automatic-planner-Miro-Icon.ico");
            // Ustaw focus na textboxInput po starcie
            this.Load += (s, e) => textBoxInput.Focus();

            // Pierwsza linia terminala i pierwsze pytanie
            AddLineToTerminal("Welcome to the Planner application");
            AddLineToTerminal("This application is designed to help you plan your work - load effectively at the XXXXXXX department.");
            AddLineToTerminal("It will assist you in determining the best order slots based on the available time at the Formeren station and Ready locations.");
            AddLineToTerminal(wizardQuestions[0]);
            Terminal.OnWriteLine += msg => AddLineToTerminal(msg);

        }
        // ---- HELPER METHODS ----
        private static bool TryParseIntInRange(string s, int min, int max, out int value)
        {
            if (int.TryParse(s, out value))
                return value >= min && value <= max;
            value = 0;
            return false;
        }

        private static bool TryParseNonNegativeInt(string s, out int value)
        {
            if (int.TryParse(s, out value))
                return value >= 0;
            value = 0;
            return false;
        }

        private static bool TryParseLayoutHK(string s, out string layout)
        {
            layout = (s ?? "").Trim().ToUpperInvariant();
            return layout == "H" || layout == "K";
        }


        private void AddLineToTerminal(string line)
        {
            if (textBoxTerminal.Text.Length > 0)
                textBoxTerminal.AppendText(Environment.NewLine);
            textBoxTerminal.AppendText(line);
            textBoxTerminal.SelectionStart = textBoxTerminal.Text.Length;
            textBoxTerminal.ScrollToCaret();
        }

        // Po zakończeniu wizardu:
        private async void RunPlannerWithAnswers(List<string> wizardAnswers, List<int> categoryCounts)
        {
            int formerenStationAmount = int.Parse(wizardAnswers[0]);
            int readyLocationAmount = int.Parse(wizardAnswers[1]);
            int scenario = int.Parse(wizardAnswers[2]);
            string layoutInput = wizardAnswers[3];

            // Tworzysz obiekt/planner i przekazujesz te dane:
            var planner = new RunPlannerAsync();
            await planner.Run(
                formerenStationAmount,
                readyLocationAmount,
                scenario,
                layoutInput,
                categoryCounts // <-- Twoje ilości slotów
            );
        }

        private void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string input = textBoxInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AddLineToTerminal("> " + input);
            textBoxInput.Clear();

            // 0..3 = pytania wstępne
            if (wizardStep < wizardQuestions.Length)
            {
                switch (wizardStep)
                {
                    case 0: // How many Forming Stations?
                        if (!TryParseIntInRange(input, 1, 5, out _))
                        {
                            AddLineToTerminal("Please enter a whole number between 1 and 5 (forming stations).");
                            return; // nie idziemy dalej
                        }
                        wizardAnswers.Add(input);
                        break;

                    case 1: // How many Ready Locations?
                        if (!TryParseIntInRange(input, 1, 16, out _))
                        {
                            AddLineToTerminal("Please enter a whole number between 1 and 16 (ready locations).");
                            return;
                        }
                        wizardAnswers.Add(input);
                        break;

                    case 2: // Choose scenario (1,2,3)
                        if (!TryParseIntInRange(input, 1, 3, out _))
                        {
                            AddLineToTerminal("Scenario must be 1, 2, or 3.");
                            return;
                        }
                        wizardAnswers.Add(input);
                        break;

                    case 3: // Choose layout: H or K
                        if (!TryParseLayoutHK(input, out var layout))
                        {
                            AddLineToTerminal("Layout must be 'H' or 'K'.");
                            return;
                        }
                        wizardAnswers.Add(layout); // zapisujemy już jako H/K
                        break;
                }

                // jeśli przeszliśmy walidację – przejdź do następnego pytania
                wizardStep++;
                if (wizardStep < wizardQuestions.Length)
                {
                    AddLineToTerminal(wizardQuestions[wizardStep]);
                }
                else
                {
                    // Start zbierania ilości per kategoria
                    AddLineToTerminal("Wizard completed! Now enter number of slots for each category:");
                    AddLineToTerminal($"How many slots for {categoryNames[categoryInputStep]}?");
                }
                return;
            }

            // ---- Ilości per kategoria ----
            if (categoryInputStep < categoryNames.Count)
            {
                if (!TryParseNonNegativeInt(input, out int slots))
                {
                    AddLineToTerminal("Please enter a non‑negative whole number (0 or more).");
                    return;
                }

                categoryCounts.Add(slots);
                categoryInputStep++;

                if (categoryInputStep < categoryNames.Count)
                {
                    AddLineToTerminal($"How many slots for {categoryNames[categoryInputStep]}?");
                }
                else
                {
                    AddLineToTerminal("All category slot numbers collected! Running planner...");
                    textBoxInput.Enabled = false;

                    // wystartuj logikę
                    RunPlannerWithAnswers(wizardAnswers, categoryCounts);
                }
            }
        }
    }
}