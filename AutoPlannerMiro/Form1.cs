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
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                string input = textBoxInput.Text.Trim();
                if (!string.IsNullOrEmpty(input))
                {
                    AddLineToTerminal("> " + input);
                    textBoxInput.Clear();

                    if (wizardStep < wizardQuestions.Length)
                    {
                        wizardAnswers.Add(input); // Zbieraj odpowiedzi z wizardu
                        wizardStep++;
                        if (wizardStep < wizardQuestions.Length)
                        {
                            AddLineToTerminal(wizardQuestions[wizardStep]);
                        }
                        else
                        {
                            AddLineToTerminal("Wizard completed! Now enter number of slots for each category:");
                            AddLineToTerminal($"How many slots for {categoryNames[categoryInputStep]}?");
                        }
                    }
                    // Kiedy kończysz wizard i zaczynasz wpisywać ilości dla kategorii:
                    else if (categoryInputStep < categoryNames.Count)
                    {
                        if (int.TryParse(input, out int slots))
                        {
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

                                // Tu wywołujesz logikę planowania, przekazując wizardAnswers i categoryCounts
                                RunPlannerWithAnswers(wizardAnswers, categoryCounts);
                            }
                        }
                        else
                        {
                            AddLineToTerminal("Please type a valid number for slots!");
                        }
                    }
                }
            }
        }
    }
}