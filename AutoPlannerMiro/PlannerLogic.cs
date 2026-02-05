using AutoPlannerMiro;
using AutoPlannerMiroUI;
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
            Dictionary<int, double[]> scenarioWeights,
            int readyMinute, 
            Dictionary<string, int> priorityCounts,
            Dictionary<string, List<(int idealMinute, int tolMin)>> priorityWindows)
        {
            if (priorityCounts != null && priorityWindows != null)
            {
                var hardFails = new List<string>();

                foreach (var (key, remaining) in priorityCounts)
                {
                    if (remaining <= 0) continue;

                    if (!priorityWindows.TryGetValue(key, out var wins) || wins.Count == 0)
                    {
                        hardFails.Add($"{key}: remaining={remaining}, windows=0");
                        continue;
                    }

                    int maxEnd = wins.Max(w => w.idealMinute + w.tolMin);
                    if (maxEnd < readyMinute)
                    {
                        string missed = string.Join(",", wins.Select(w => $"[{w.idealMinute - w.tolMin}-{w.idealMinute + w.tolMin}]"));
                        hardFails.Add($"{key}: remaining={remaining}, maxEnd={maxEnd}, windows={missed}");
                    }
                }

                if (hardFails.Count > 0)
                {
                    string msg = $"MISSED priority window(s) @ {readyMinute}: {string.Join("; ", hardFails)}";
                  //  Terminal.WriteLine("ERROR: " + msg);
                    throw new InvalidOperationException(msg);
                }
            }

            if (priorityCounts != null && priorityWindows != null)
            {
                foreach (var cat in categories.Where(c => c.Count > 0))
                {
                    string key = cat.Category;

                    if (priorityCounts.TryGetValue(key, out int remaining) && remaining > 0 &&
                        priorityWindows.TryGetValue(key, out var wins) && wins.Count > 0)
                    {
                        int idx = wins.FindIndex(w =>
                            readyMinute >= (w.idealMinute - w.tolMin) &&
                            readyMinute <= (w.idealMinute + w.tolMin));

                        if (idx >= 0)
                        {
                            var w = wins[idx];
                            Terminal.WriteLine($"PRIO PICK: {key} @ {readyMinute} (ideal={w.idealMinute}, tol={w.tolMin})");


                            priorityCounts[key] = Math.Max(0, remaining - 1);
                            wins.RemoveAt(idx);

                            return cat;
                        }
                    }
                }
            }

            int group = DrawGroupByScenario(scenario, rng, scenarioWeights);

            var groupCategories = categories
                .Where(cat => cat.DurationGroup == group && cat.Count > 0)
                .ToList();

            if (groupCategories.Count == 0)
            {
                var fallback = categories.Where(c => c.Count > 0).ToList();
                if (fallback.Count == 0) return null;

                groupCategories = fallback;
            }

            var nonPriorityNow = groupCategories.Where(c =>
            {
                if (priorityCounts == null || priorityWindows == null) return true;

                if (!priorityCounts.TryGetValue(c.Category, out int remaining) || remaining <= 0)
                    return true;

                if (!priorityWindows.TryGetValue(c.Category, out var wins) || wins.Count == 0)
                    return true;

                return !wins.Any(w => readyMinute < (w.idealMinute - w.tolMin));
            }).ToList();

            if (nonPriorityNow.Count == 0)
            {
                return null;
            }

            return nonPriorityNow[rng.Next(nonPriorityNow.Count)];
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

        public void PlanToFormerenStation(
            List<FormerenStation> formerenStations,
            List<CategoryCount> categories,
            LevelOfMatching matching,
            Random rng,
            Dictionary<string, int> priorityCounts,
            Dictionary<string, List<(int idealMinute, int tolMin)>> priorityWindows)
        {
            var bestStation = FindStationWithLowestMaxTimeBusy(formerenStations);
            int readyMinute = bestStation.TimeBusy.Count > 0 ? bestStation.TimeBusy.Max() + 1 : 1;
            //Terminal.WriteLine($"TRY PICK @ {readyMinute} | bestStation={bestStation.FormerenStationID}");


            var firstAvailableCategory = PickOrderByScenario(
                categories,
                matching.ScenarioUsed,
                rng,
                matching.ScenarioWeights,
                readyMinute,
                priorityCounts,
                priorityWindows
            );
            if (firstAvailableCategory == null)
            {

               // Terminal.WriteLine("No selectable category (probably none left) — finishing planning step.");
                return;
            }

              //  Terminal.WriteLine($"PICKED: {firstAvailableCategory.Category} (dur={firstAvailableCategory.Duration}) @ station {bestStation.FormerenStationID}");
            int duration = firstAvailableCategory.Duration;
            int previousMax = bestStation.TimeBusy.Count > 0 ? bestStation.TimeBusy.Max() : 0;
            int orderStart = previousMax + 1;
            var orderRange = TimeRange.FromStartAndDuration(orderStart, duration);
            if (!IsSlotAvailable(bestStation.TimeBusy, orderRange.Start, orderRange.End))
            {
                string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] No available slot on station {bestStation.FormerenStationID} " +
                             $"for category {firstAvailableCategory.Category}, duration {duration} (start={orderStart}, end={orderRange.End}).";
                ErrorLogs.Add(msg);
                throw new InvalidOperationException(msg);
            }

            // We take order for further processing
            //Terminal.WriteLine($"Working with category: {firstAvailableCategory.Category}, We still have: {firstAvailableCategory.Count - 1}");
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

            int newY = prevY + (prevHeight / 2) + (duration / 2) + margin;


            MarkSlotBusy(bestStation.TimeBusy, orderRange.Start, orderRange.End);
            firstAvailableCategory.Count--;
            bestStation.OrdersAdded.Push((
            firstAvailableCategory.Category,
            bestStation.x,
            newY,
            firstAvailableCategory.Color,
            orderStart,
            orderRange.End
            ));
            OrdersMovedFromTheStationList.Add(new OrderInfo
            {
                Category = firstAvailableCategory.Category,
                X = bestStation.x,
                Y = newY,
                Color = firstAvailableCategory.Color,
                Start = orderStart,
                End = orderRange.End
            });

    }

    public static bool IsSlotAvailable(HashSet<int> timeBusy, int orderStart, int orderEnd)
    {
        for (int minute = orderStart; minute <= orderEnd; minute++)
            if (timeBusy.Contains(minute))
                return false;
        return true;
    }

    public static void MarkSlotBusy(HashSet<int> timeBusy, int orderStart, int orderEnd)
    {
        for (int minute = orderStart; minute <= orderEnd; minute++)
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
public static void TransferOrdersToReadyLocations(
    List<FormerenStation> formerenStations,
    List<ReadyLocation> readyLocations,
    List<CategoryCount> categories,
    int levelOfSevernity,
    int maxSimultaneousLoading,
    List<DockAssignment> dockAssignments)
{
    var station = formerenStations
        .Where(s => s.OrdersAdded.Count > 0)
        .OrderBy(s => s.OrdersAdded.Peek().orderStart)
        .FirstOrDefault();

    if (station == null) return;

    var order = station.OrdersAdded.Peek();

    int duration = categories.First(cat => cat.Category == order.OrderCategory).Duration;
    var orderRange = new TimeRange(order.orderStart, order.orderEnd);
    int loadingDuration = LevelOfMatching.GetLoadingTimeBySeverity(levelOfSevernity);
    var loadingRange = orderRange.NextAfter(loadingDuration);


    var dockIdsForThisStation = dockAssignments
        .Where(d => d.FormerenStationId == station.FormerenStationID)
        .Select(d => d.DockId)
        .ToHashSet();

    ReadyLocation ready = null;

    foreach (var loc in readyLocations
        .Where(r => dockIdsForThisStation.Contains(r.ReadyLocationID))
        .OrderBy(_ => Guid.NewGuid()))
    {
        if (IsSlotAvailable(loc.TimeBusy, orderRange.Start, loadingRange.End))
        {
            ready = loc;
            break;
        }
    }

    if (ready == null)
    {
        string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} No empty place! Tried {orderRange.Start}-{loadingRange.End} on any of: {string.Join(",", dockIdsForThisStation)}";
        ErrorLogs.Add(msg);
        throw new InvalidOperationException(msg);
    }

    int x = (20 + ready.ReadyLocationID) * 100;


    if (!IsLoadingSlotAvailable(readyLocations, loadingRange.Start, loadingRange.End, maxSimultaneousLoading))
    {
        string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Loading slots exceed maxParallel={maxSimultaneousLoading} for time {loadingRange.Start}-{loadingRange.End}";
        ErrorLogs.Add(msg);
        throw new InvalidOperationException(msg);
    }

    MarkSlotBusy(ready.TimeBusy, orderRange.Start, loadingRange.End);

    station.OrdersAdded.Pop();

    // order shape
    ready.OrdersAdded.Push((
        order.OrderCategory,
        x,
        order.y,
        order.Color,
        orderRange.Start,
        orderRange.End
    ));

    int loadingY = order.y + duration / 2 + loadingDuration / 2 + 1;

    // loading shape
    ready.OrdersAdded.Push((
        "Loading time",
        x,
        loadingY,
        "#000000",
        loadingRange.Start,
        loadingRange.End
    ));
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