using AutoPlannerMiro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RestSharp;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AutoPlannerMiro
{
    internal class RunPlannerAsync
    {

        const string Author = "|| Made by: Michal Domagala";
        const string Contact = "  || Visit my LinkedIn profile: https://www.linkedin.com/in/michal-domagala-b0147b236/";
        const string Version = "|| Version: 1.18";
        const string TotalTries = $"|| Total tries: ";
        const string LevelOfSevernity2 = "|| Level of Severnity: ";

        public static List<CategoryCount> DeepCopyCategories(List<CategoryCount> source)
        {
            return source.Select(cat => new CategoryCount
            {
                Category = cat.Category,
                Duration = cat.Duration,
                Count = cat.Count,
                DurationGroup = cat.DurationGroup,
                Color = cat.Color
            }).ToList();
        }

        public static List<FormerenStation> DeepCopyFormerenStations(List<FormerenStation> source)
        {
            return source.Select(st => new FormerenStation
            {
                FormerenStationID = st.FormerenStationID,
                TimeAvailableFormeren = st.TimeAvailableFormeren,
                x = st.x,
                y = st.y,
                orderStart = st.orderStart,
                orderEnd = st.orderEnd,
                TimeBusy = new HashSet<int>(st.TimeBusy),
                OrdersAdded = new Stack<(string, int, int, string, int, int)>(st.OrdersAdded.Reverse())
            }).ToList();
        }

        public static List<ReadyLocation> DeepCopyReadyLocations(List<ReadyLocation> source)
        {
            return source.Select(loc => new ReadyLocation
            {
                ReadyLocationID = loc.ReadyLocationID,
                TimeAvailableReady = loc.TimeAvailableReady,
                x = loc.x,
                y = loc.y,
                orderStart = loc.orderStart,
                orderEnd = loc.orderEnd,
                TimeBusy = new HashSet<int>(loc.TimeBusy),
                OrdersAdded = new Stack<(string, int, int, string, int, int)>(loc.OrdersAdded.Reverse())
            }).ToList();
        }

        public static int ReadIntFromConsole(string prompt)
        {
            int result;
            while (true)
            {
                Terminal.WriteLine(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out result))
                {
                    if (result > -1)
                        return result;
                    else
                        Terminal.WriteLine("Please type in a valid number greater than 0.");
                }
                else
                {
                    Terminal.WriteLine("Invalid format, only integer can be accepted.");
                }
            }
        }
        public static int ReadIntFromConsoleScenario(string prompt)
        {
            int result;
            while (true)
            {
                Terminal.WriteLine(prompt);
                string input = Console.ReadLine();
                if (int.TryParse(input, out result))
                {
                    if (result > 0 && result < 4)
                        return result; // Sukces – zwracamy wynik
                    else
                        Terminal.WriteLine("Please type in a valid number greater than 0.");
                }
                else
                {
                    Terminal.WriteLine("Invalid format, only integer can be accepted.");
                }
            }
        }
        public static string ReadDepartmentFromConsole(string prompt)
        {
            while (true)
            {
                Terminal.WriteLine(prompt);
                string input = Console.ReadLine()?.Trim().ToUpper();
                if (input == "H" || input == "K")
                {
                    return input; // Zwraca "H" lub "K"
                }
                else
                {
                    Terminal.WriteLine("Please type in a valid department: H or K.");
                }
            }
        }

        public static async Task SendMiroShapeAsync(string content, int x, int y, string fillColor, int height, string apiKey)
        {

            // Token have to be filled up ( temporary token for testing purposes)
            var options = new RestClientOptions("https://api.miro.com/v2/boards/uXjVIgiE9aY%3D/shapes");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {apiKey}");

            string body = $"{{\"data\":{{\"content\":\"{content}\",\"shape\":\"rectangle\"}},\"position\":{{\"x\":{x},\"y\":{y}}},\"geometry\":{{\"height\":{height},\"width\":100}},\"style\":{{\"fillColor\":\"{fillColor}\"}}}}";
            request.AddJsonBody(body, false);

            var response = await client.PostAsync(request);

            Terminal.WriteLine(response.Content);
        }




        public async Task Run(int formerenStationAmount, int readyLocationAmount, int scenario, string layoutInput, List<int> categoryCounts)
        {


            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string apiKey = config["Miro:ApiKey"];

            int OrderSlotAmmount = 0;
            List<CategoryCount> Categories = new List<CategoryCount>
        {
            new CategoryCount{ Category = "Order_Type1",      Duration=60 ,   Count =0, DurationGroup= 1, Color = "#f5f6f8"},
            new CategoryCount{ Category = "Order_Type2",      Duration=90 ,   Count =0, DurationGroup= 1, Color = "#d5f692"},
            new CategoryCount{ Category = "Order_Type3",      Duration=120 ,  Count =0, DurationGroup= 2, Color = "#d0e17a"},
            new CategoryCount{ Category = "Order_Type4",      Duration=150 ,  Count =0, DurationGroup= 2, Color = "#93d275"},
            new CategoryCount{ Category = "Order_Type5",      Duration=180 ,  Count =0, DurationGroup= 2, Color = "#67c6c0"},
            new CategoryCount{ Category = "Order_Type6",      Duration=210 ,  Count =0, DurationGroup= 2, Color = "#23bfe7"},
            new CategoryCount{ Category = "Order_Type7",      Duration=240 ,  Count =0, DurationGroup= 2, Color = "#a6ccf5"},
            new CategoryCount{ Category = "Order_Type8",      Duration=270 ,  Count =0, DurationGroup= 3, Color = "#7b92ff"},
            new CategoryCount{ Category = "Order_Type9",      Duration=300 ,  Count =0, DurationGroup= 3, Color = "#fff9b1"},
            new CategoryCount{ Category = "Order_Type10",     Duration=330 ,  Count =0, DurationGroup= 3, Color = "#f5d128"},
            new CategoryCount{ Category = "Order_Type11",     Duration=240 ,  Count =0, DurationGroup= 2, Color = "#ff9d48"},
            new CategoryCount{ Category = "Order_Type12",     Duration=270 ,  Count =0, DurationGroup= 3, Color = "#f16c7f"},
            new CategoryCount{ Category = "Order_Type13",     Duration=180 ,  Count =0, DurationGroup= 2, Color = "#ea94bb"},
            //new CategoryCount{ Category = "VE(A)",      Duration=60 ,   Count =0, DurationGroup= 1, Color = "#f5f6f8"},
            //new CategoryCount{ Category = "VE(B)",      Duration=90 ,   Count =0, DurationGroup= 1, Color = "#d5f692"},
            //new CategoryCount{ Category = "E(A)",       Duration=120 ,  Count =0, DurationGroup= 2, Color = "#d0e17a"},
            //new CategoryCount{ Category = "E(B)",       Duration=150 ,  Count =0, DurationGroup= 2, Color = "#93d275"},
            //new CategoryCount{ Category = "M(A)",       Duration=180 ,  Count =0, DurationGroup= 2, Color = "#67c6c0"},
            //new CategoryCount{ Category = "M(B)",       Duration=210 ,  Count =0, DurationGroup= 2, Color = "#23bfe7"},
            //new CategoryCount{ Category = "D(A)",       Duration=240 ,  Count =0, DurationGroup= 2, Color = "#a6ccf5"},
            //new CategoryCount{ Category = "D(B)",       Duration=270 ,  Count =0, DurationGroup= 3, Color = "#7b92ff"},
            //new CategoryCount{ Category = "VD(A)",      Duration=300 ,  Count =0, DurationGroup= 3, Color = "#fff9b1"},
            //new CategoryCount{ Category = "VD(B)",      Duration=330 ,  Count =0, DurationGroup= 3, Color = "#f5d128"},
            //new CategoryCount{ Category = "Container",  Duration=240 ,  Count =0, DurationGroup= 2, Color = "#ff9d48"},
            //new CategoryCount{ Category = "SE",         Duration=270 ,  Count =0, DurationGroup= 3, Color = "#f16c7f"},
            //new CategoryCount{ Category = "BE(A)",      Duration=180 ,  Count =0, DurationGroup= 2, Color = "#ea94bb"},
            //new CategoryCount{ Category = "BE(B)",      Duration=270 ,  Count =0, DurationGroup= 3, Color = "#ffcee0"},
            //new CategoryCount{ Category = "NL",         Duration=270 ,  Count =0, DurationGroup= 3, Color = "#b384bb"},
         };


            int FormerenStationAmmount = formerenStationAmount;
            int ReadyLocationAmmount = readyLocationAmount;
            Random rng = new Random();

            var categoryEnumerator = categoryCounts.GetEnumerator();
            foreach (var cat in Categories)
            {
                if (!categoryEnumerator.MoveNext())
                    break; // lub rzucić błąd jak nie pasuje ilość
                cat.Count = categoryEnumerator.Current;
            }


            if ((Author + Contact).Sum(c => (int)c) != 10004)
            {
                await SendMiroShapeAsync(
                "DO NOT MODIFY MY CODE BRO!!!",
                100,
                100,
                "#FF0000",
                1000,
                apiKey
                );
                Environment.Exit(1);
            }

            var formerenStations = CreatorFormerenStation.CreatorFormerenStations(FormerenStationAmmount);
            var readyLocation = CreatorReadyLocation.CreatorReadyLocations(ReadyLocationAmmount);
            var matching = new LevelOfMatching(scenario);
            var readyStatus = FileHelpers.ReadReadyLocationStatus("ready_locations_status.csv");



            // PLANNER SETTINGS
            bool success = false;
            int maxTries = 5000;
            int tries = 0;
            int baseMaxSimultaneousLoading;
            if (layoutInput == "H")
            { baseMaxSimultaneousLoading = 4; }
            else
            { baseMaxSimultaneousLoading = 3; }
            int MaxSimultaneousLoading;


            var levelOfMatching = new LevelOfMatching(scenario);
            if ((Author + Contact).Sum(c => (int)c) != 10004)
            { Environment.Exit(1); }

            // We are going to use PlannerLogic class to plan the orders based on the user input.
            PlannerLogic planner = new PlannerLogic();

            var OriginalCategories = DeepCopyCategories(Categories);
            var OriginalFormerenStations = CreatorFormerenStation.CreatorFormerenStations(FormerenStationAmmount);
            var OriginalReadyLocation = CreatorReadyLocation.CreatorReadyLocations(ReadyLocationAmmount);


            foreach (var location in OriginalReadyLocation)
            {
                if (readyStatus.TryGetValue(location.ReadyLocationID, out int occupiedUntil))
                {
                    Terminal.WriteLine($"LocationID: {location.ReadyLocationID}, I set up time to: {occupiedUntil}");
                    for (int i = 0; i < occupiedUntil; i++)
                    {
                        location.TimeBusy.Add(i);
                    }
                }
                else
                {
                
                    Terminal.WriteLine($"LocationID: {location.ReadyLocationID} I do not see anything there, no changes done.");
                 
                }
            }


            while (!success && tries < maxTries)
            {
                tries++;
                var tmpCategories = DeepCopyCategories(OriginalCategories);
                var tmpFormerenStations = DeepCopyFormerenStations(OriginalFormerenStations);
                var tmpReadyLocation = DeepCopyReadyLocations(OriginalReadyLocation);
                var dockAssignments = PlannerLogic.AssignDocks(tmpFormerenStations.Count, layoutInput);

                switch (matching.LevelOfSevernity)
                {
                    case 1:
                    case 2:
                        MaxSimultaneousLoading = baseMaxSimultaneousLoading;
                        break;
                    case 3:
                        MaxSimultaneousLoading = baseMaxSimultaneousLoading + 1;
                        break;
                    case 4:
                        MaxSimultaneousLoading = baseMaxSimultaneousLoading + 2;
                        break;
                    default:
                        MaxSimultaneousLoading = baseMaxSimultaneousLoading;
                        break;
                }
                // We prepare clean data for the planner


                try
                {
                    planner.CheckIfEnoughTimeAvailable(tmpFormerenStations, tmpCategories);
                    while (tmpCategories.Sum(cat => cat.Count) > 0)
                    {
                        planner.PlanToFormerenStation(tmpFormerenStations, tmpCategories, matching, rng);
                    }
                    Terminal.WriteLine("All orders have been given!");

                    while (!tmpFormerenStations.All(station => station.OrdersAdded.Count == 0))
                    {
                        PlannerLogic.TransferOrdersToReadyLocations(tmpFormerenStations, tmpReadyLocation, tmpCategories, matching.LevelOfSevernity, MaxSimultaneousLoading, dockAssignments);
                    }
                    Terminal.WriteLine("All orders have been moved!");


                    foreach (var order in planner.OrdersMovedFromTheStationList)
                    {
                        await SendMiroShapeAsync(
                            order.Category,
                            order.X,
                            order.Y,
                            order.Color,
                            order.End - order.Start,
                            apiKey
                        );
                    }

                    foreach (var location in tmpReadyLocation)
                    {
                        foreach (var order in location.OrdersAdded)
                        {
                            string content = order.OrderCategory;
                            int x = order.x;
                            int y = order.y;
                            string fillColor = order.Color;

                            int duration;
                            if (order.OrderCategory == "Loading time")
                                duration = PlannerLogic.GetLoadingTimeBySeverity(matching.LevelOfSevernity);
                            else
                                duration = Categories.First(cat => cat.Category == order.OrderCategory).Duration;

                            int height = duration;
                            await SendMiroShapeAsync(content, x, y, fillColor, height, apiKey);
                        }
                    }




                    planner.OrdersMovedFromTheStationList.Clear();

                    if (Author != "|| Made by: Michal Domagala" || Author.Length != 27)
                    {
                        Terminal.WriteLine("DO NOT MODIFY MY CODE BRO!!!");
                        await SendMiroShapeAsync(
                            "DO NOT MODIFY MY CODE BRO!!!",
                            0,
                            0,
                            "#FF0000",
                            100000,
                            apiKey
                        );

                    }

                    string authorNote = $"{Author} {Contact} {Version} {TotalTries}{matching.TotalTries} {LevelOfSevernity2}{matching.LevelOfSevernity}";
                    await SendMiroShapeAsync(
                        authorNote,
                        -240,
                        0,
                        "#FFFFFF",
                        380,
                        apiKey
                    );
                    success = true;
                }
                catch (InvalidOperationException ex)
                {
                    {
                        Terminal.WriteLine("Error: " + ex.Message);
                        matching.IncrementTries();
                        Terminal.WriteLine($"Total tries: {matching.TotalTries}");
                        planner.OrdersMovedFromTheStationList.Clear();
                        if (tries >= 2000 && tries < 3000)
                        {
                            matching.UpdateSevernity(2);
                            Terminal.WriteLine("Severity level increased to 2 due to 2000 unsuccessful tries.");
                        }
                        if (tries >= 3000 && tries < 4000)
                        {
                            matching.UpdateSevernity(3);
                            Terminal.WriteLine("Severity level increased to 3 due to 3000 unsuccessful tries.");
                        }
                        if (tries >= 4000)
                        {
                            matching.UpdateSevernity(4);
                            Terminal.WriteLine("Severity level increased to 4 due to 4000 unsuccessful tries.");
                        }
                    }
                }
            }
            if (!success)
            {
                matching.IncrementTries();
                Terminal.WriteLine("Failed to plan orders after maximum attempts. Please check your input and try again.");
                Console.ReadLine();
            }
            else
            {
                matching.IncrementTries();
                Terminal.WriteLine($"Orders have been successfully planned and sent to Miro! Total tries: {matching.TotalTries}");
                Console.ReadLine();
            }
            if (PlannerLogic.ErrorLogs.Any())
            {
                string reportFile = $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
                using (var writer = new StreamWriter(reportFile))
                {

                    writer.WriteLine("Timestamp,ErrorType,Message,Category,Duration,AdditionalInfo,x,x2,TimeRange");
                    foreach (var log in PlannerLogic.ErrorLogs)
                    {
                        writer.WriteLine(log);
                    }
                    writer.WriteLine();


                }
                string reportFile2 = $"report_orders_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
                using (var writer = new StreamWriter(reportFile2))
                {
                    writer.Write("Category,Count,Duration");
                    writer.WriteLine();
                    foreach (var cat in Categories)
                    {
                        writer.WriteLine($"{cat.Category},{cat.Count},{cat.Duration}");
                    }
                    Terminal.WriteLine($"Error log + order count exported to 2 files: {reportFile}");
                }
            }

        }

    }







}
