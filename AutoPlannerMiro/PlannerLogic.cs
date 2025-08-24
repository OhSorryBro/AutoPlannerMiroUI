using AutoPlannerMiro;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AutoPlannerMiro
{ 
public class PlannerLogic
{
    public static List<string> ErrorLogs = new List<string>();

    public class DockAssignment
    {
        public int DockId { get; set; }
        public int FormerenStationId { get; set; }
    }

    public static List<DockAssignment> AssignDocks(int numberOfFormerenStations, string layoutType)
    {
        var dockAssignments = new List<DockAssignment>();

        Dictionary<int, List<(int startDock, int endDock)>> mapping;


        if (layoutType.ToUpper() == "H")
        {
            mapping = new Dictionary<int, List<(int, int)>>()
        {
            { 1, new List<(int, int)> { (1, 13) } },
            { 2, new List<(int, int)> { (1, 6), (7, 13) } },
            { 3, new List<(int, int)> { (1, 3), (4, 8), (9, 13) } },
            { 4, new List<(int, int)> { (1, 2), (3, 5), (6, 8), (9, 13) } },
            { 5, new List<(int, int)> { (1, 2), (3, 4), (5, 6), (7, 9), (10, 12) } },
        };
        }
        else if (layoutType.ToUpper() == "K")
        {
            if (numberOfFormerenStations > 4)
                throw new ArgumentException("Layout K supports max 4 FormerenStations");

            mapping = new Dictionary<int, List<(int, int)>>()
        {
            { 1, new List<(int, int)> { (2, 8) } },
            { 2, new List<(int, int)> { (2, 5), (5, 8) } },
            { 3, new List<(int, int)> { (2, 3), (4, 6), (7, 8) } },
            { 4, new List<(int, int)> { (1, 2), (3, 4), (5, 6), (7, 8) } },
        };
        }
        else
        {
            throw new ArgumentException("Unknown layout type. Use 'H' or 'K'.");
        }

        if (!mapping.ContainsKey(numberOfFormerenStations))
            throw new ArgumentException($"Unsupported number of FormerenStations for layout {layoutType}");

        var ranges = mapping[numberOfFormerenStations];

        for (int i = 0; i < ranges.Count; i++)
        {
            var (start, end) = ranges[i];
            for (int dock = start; dock <= end; dock++)
            {
                dockAssignments.Add(new DockAssignment
                {
                    DockId = dock,
                    FormerenStationId = i + 1 // ID1, ID2, ...
                });
            }
        }

        return dockAssignments;
    }

    public static FormerenStation FindStationWithLowestMaxTimeBusy(List<FormerenStation> stations)
    {
        return stations
            .OrderBy(station => station.TimeBusy.Count > 0 ? station.TimeBusy.Max() : 0)
            .First();
    }
    public static ReadyLocation FindReadyLocationWithLowestMaxTimeBusy(List<ReadyLocation> readyLocations)
    {
        return readyLocations
            .OrderBy(location => location.TimeBusy.Count > 0 ? location.TimeBusy.Max() : 0)
            .First();
    }





    public int DrawGroupByScenario(int scenario, Random rng, Dictionary<int, double[]> scenarioWeights)
    {
        var weights = scenarioWeights[scenario];
        double roll = rng.NextDouble();
        if (roll < weights[0])
            return 1;
        else if (roll < weights[0] + weights[1])
            return 2;
        else
            return 3;
    }
    public CategoryCount PickOrderByScenario(
        List<CategoryCount> categories,
        int scenario,
        Random rng,
        Dictionary<int, double[]> scenarioWeights)
    {
        while (true)
        {
            int group = DrawGroupByScenario(scenario, rng, scenarioWeights);

            var groupCategories = categories
                .Where(cat => cat.DurationGroup == group && cat.Count > 0)
                .ToList();

            if (groupCategories.Count > 0)
            {
                return groupCategories[rng.Next(groupCategories.Count)];
            }
            if (!categories.Any(cat => cat.Count > 0))
                return null;
        }
    }
    public class OrderInfo
    {
        public string Category { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
    public List<OrderInfo> OrdersMovedFromTheStationList { get; } = new List<OrderInfo>();

    public void PlanToFormerenStation(List<FormerenStation> formerenStations,
                                        List<CategoryCount> categories,
                                        LevelOfMatching matching,
                                        Random rng)
    {
        var bestStation = FindStationWithLowestMaxTimeBusy(formerenStations);
        var firstAvailableCategory = PickOrderByScenario(categories, matching.ScenarioUsed, rng, matching.ScenarioWeights);


        if (firstAvailableCategory != null)
        {
            int duration = firstAvailableCategory.Duration;
            bool canPlace = formerenStations.Any(station =>
                                            IsSlotAvailable(station.TimeBusy,
                                            (station.TimeBusy.Count > 0 ? station.TimeBusy.Max() + 1 : 1),
                                            (station.TimeBusy.Count > 0 ? station.TimeBusy.Max() + 1 : 1) + duration - 1)
                                            );
            if (!canPlace)
            {
                string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] No available worker slot for category {firstAvailableCategory.Category}, duration {duration} – try again!";
                ErrorLogs.Add(msg);
                throw new InvalidOperationException(msg);

            }
                // We take order for further processing
                //Terminal.WriteLine($"Working with category: {firstAvailableCategory.Category}, We still have: {firstAvailableCategory.Count - 1}");
                int previousMax = bestStation.TimeBusy.Count > 0 ? bestStation.TimeBusy.Max() : 0;
            int prevY = 1; // We set up prevY to 1, just in case if order is 1st.

            // We assume that heigh of last order = Duration last category order
            // y parameter is quaite scetchy because it is calculated base on the middle of the schape (same as x, but x is constant per station)
            int prevHeight = 0;
            if (bestStation.OrdersAdded.Count > 0)
            {
                var prevOrder = bestStation.OrdersAdded.Peek();
                prevY = prevOrder.y;
                prevHeight = categories.FirstOrDefault(cat => cat.Category == prevOrder.OrderCategory)?.Duration ?? 0;
            }
            int margin = 1;
            var orderStart = previousMax + 1;
            var orderEnd = orderStart + duration - 1;
            int newY = prevY + (prevHeight / 2) + (duration / 2) + margin;

            // end of y parameter calculation

            // Filling up TimeBusy for the station
            for (int i = 1; i <= duration; i++)
            {
                bestStation.TimeBusy.Add(previousMax + i);
            }
            // We take 1 from Count property of the take category
            firstAvailableCategory.Count--;
            // We add Order to the station stack
            bestStation.OrdersAdded.Push((
            firstAvailableCategory.Category,
            bestStation.x,
            newY,
            firstAvailableCategory.Color,
            orderStart,
            orderEnd
            ));
            OrdersMovedFromTheStationList.Add(new OrderInfo
            {
                Category = firstAvailableCategory.Category,
                X = bestStation.x,
                Y = newY,
                Color = firstAvailableCategory.Color,
                Start = orderStart,
                End = orderEnd
            });
        }
        else
        {
                Terminal.WriteLine("No more orders to process in this scenario.");
        }

    }

    public static bool IsSlotAvailable(HashSet<int> timeBusy, int orderStart, int orderEnd)
    {
        for (int minute = orderStart; minute < orderEnd; minute++)
            if (timeBusy.Contains(minute))
                return false;
        return true;
    }

    public static void MarkSlotBusy(HashSet<int> timeBusy, int orderStart, int orderEnd)
    {
        for (int minute = orderStart; minute < orderEnd; minute++)
            timeBusy.Add(minute);
    }

    public static bool IsLoadingSlotAvailable(List<ReadyLocation> readyLocations, int loadingStart, int loadingEnd, int maxSimultaneousLoading)
    {
        int count = 0;
        foreach (var loc in readyLocations)
        {
            foreach (var order in loc.OrdersAdded)
            {
                if (order.OrderCategory == "Loading time")
                {
                    // Sprawdź czy się nakładają
                    if (!(order.orderEnd < loadingStart || order.orderStart > loadingEnd))
                    {
                        count++;
                        if (count >= maxSimultaneousLoading)
                            return false;
                    }
                }
            }
        }
        return true;
    }
    public static void TransferOrdersToReadyLocations(List<FormerenStation> formerenStations, List<ReadyLocation> readyLocations, List<CategoryCount> categories, int levelOfSevernity, int maxSimultaneousLoading, List<DockAssignment> dockAssignments)
    {
        var station = formerenStations.FirstOrDefault(s => s.OrdersAdded.Count > 0);
        if (station == null) return;

        var order = station.OrdersAdded.Pop();
        int duration = categories.First(cat => cat.Category == order.OrderCategory).Duration;
        int orderStart = order.orderStart;
        int orderEnd = order.orderEnd;
        int loadingDuration = LevelOfMatching.GetLoadingTimeBySeverity(levelOfSevernity);
        int loadingStart = orderEnd + 1;
        int loadingEnd = loadingStart + loadingDuration - 1;

        ReadyLocation ready = null;
        // FILTRUJEMY docki przypisane do danego stanowiska
        var dockIdsForThisStation = dockAssignments
            .Where(d => d.FormerenStationId == station.FormerenStationID)
            .Select(d => d.DockId)
            .ToHashSet();


        foreach (var loc in readyLocations
                    .Where(r => dockIdsForThisStation.Contains(r.ReadyLocationID))
                    .OrderBy(_ => Guid.NewGuid()))
        {
            // Tu sprawdzamy cały zakres: orderStart ... loadingEnd
            if (IsSlotAvailable(loc.TimeBusy, orderStart, loadingEnd))
            {
                ready = loc;
                break;
            }
        }
        if (ready == null)
        {
            string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} No empty place! Tried to place at minute {orderStart}-{loadingEnd} on any of: {string.Join(",", dockIdsForThisStation)}";
            throw new InvalidOperationException(msg);
        }


        // Calculate x
        int x = (20 + ready.ReadyLocationID) * 100;

        MarkSlotBusy(ready.TimeBusy, orderStart, orderEnd);

        // We move order (with new x)
        ready.OrdersAdded.Push((
            order.OrderCategory,
            x,
            order.y,
            order.Color,
            orderStart,
            orderEnd
        ));
        int loadingY = order.y + duration / 2 + loadingDuration / 2 + 1; // That was a tricky one

        if (IsLoadingSlotAvailable(readyLocations, loadingStart, loadingEnd, maxSimultaneousLoading))
        {
            ready.OrdersAdded.Push((
                "Loading time",
                x,
                loadingY,
                "#000000",
                loadingStart,
                loadingEnd
            ));
            MarkSlotBusy(ready.TimeBusy, loadingStart, loadingEnd);
        }
        else
        {
            string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Loading slots exceed maxParallel={maxSimultaneousLoading} for time {loadingStart}-{loadingEnd}";
            ErrorLogs.Add(msg);
            throw new InvalidOperationException(msg);
        }
    }

    public void CheckIfEnoughTimeAvailable(List<FormerenStation> formerenStations, List<CategoryCount> categories)
    {

        int totalTimeNeeded = categories.Sum(cat => cat.Count * cat.Duration);
        int totalTimeAvailable = formerenStations.Sum(station => station.TimeAvailableFormeren);
        if (totalTimeNeeded > totalTimeAvailable)
        {
            throw new InvalidOperationException($"Not enough time available in Formeren stations to process all orders. Orders need {totalTimeNeeded} minutes and it is greater than available: {totalTimeAvailable} minutes")
            {

            };
        }
    }
}
}