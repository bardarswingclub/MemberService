namespace MemberService.Pages.Kursplanlegging;

using MemberService.Data.CoursePlanning;

using static CoursePlanLogic;

public class TimeplanModel
{
    public record Entry(PlannedCourse Course, Room Room, RoomSlot Slot, int Start, int End)
    {
        public DateOnly First => Course.Dates.Min();

        public DateOnly Last => Course.Dates.Max();

        /// <summary>Kolonne innenfor salen, når flere kurs går på samme tid i hver sin periode.</summary>
        public int Lane { get; set; }

        public int LaneCount { get; set; } = 1;
    }

    public record Column(Room Room, IReadOnlyList<Entry> Entries);

    public record Day(int DayOfWeek, IReadOnlyList<Column> Columns);

    public string Title { get; init; }

    /// <summary>Stedet de fleste kursene er på. Kurs andre steder markeres med sted.</summary>
    public string MainVenue { get; init; }

    public int FirstMinute { get; init; }

    public int LastMinute { get; init; }

    public IReadOnlyList<Day> Days { get; init; }

    public IReadOnlyList<DateOnly> Weeks { get; init; }

    public IReadOnlyList<Entry> Entries { get; init; }

    /// <summary>Fridager i perioden som vises, til fotnoten på semesterplanen.</summary>
    public IReadOnlyList<Holiday> Holidays { get; init; }

    public bool IsHolidayWeek(DateOnly monday) => Holidays.Any(h => h.From <= monday.AddDays(6) && h.To >= monday);

    public string Place(Entry entry)
        => string.IsNullOrWhiteSpace(entry.Room.Venue) || entry.Room.Venue == MainVenue
            ? entry.Room.Name
            : $"{entry.Room.Name}, {entry.Room.Venue}";

    public static bool HasWeek(Entry entry, DateOnly monday)
        => entry.Course.Dates.Any(d => d >= monday && d < monday.AddDays(7));

    /// <summary>
    /// Kurs på samme tid i samme sal, men i hver sin periode (f.eks. to 6-ukers kurs etter hverandre),
    /// vises i hver sin kolonne side om side.
    /// </summary>
    public static void AssignLanes(IReadOnlyList<Entry> entries)
    {
        foreach (var cluster in Connected(entries, (a, b) => a.Start < b.End && b.Start < a.End))
        {
            var periods = Connected(cluster, (a, b) => a.First <= b.Last && b.First <= a.Last)
                .OrderBy(p => p.Min(e => e.First))
                .ToList();

            for (var lane = 0; lane < periods.Count; lane++)
            {
                foreach (var entry in periods[lane])
                {
                    entry.Lane = lane;
                    entry.LaneCount = periods.Count;
                }
            }
        }
    }

    private static List<List<T>> Connected<T>(IEnumerable<T> items, Func<T, T, bool> linked)
    {
        var groups = new List<List<T>>();
        foreach (var item in items)
        {
            var touching = groups.Where(g => g.Any(other => linked(item, other))).ToList();
            var merged = touching.SelectMany(g => g).Prepend(item).ToList();
            groups.RemoveAll(touching.Contains);
            groups.Add(merged);
        }
        return groups;
    }

    public static TimeplanModel Create(CoursePlan plan)
    {
        var rooms = RoomsJson.Parse(plan.RoomsJson);

        var entries = plan.Courses
            .Select(c =>
            {
                var room = rooms.FirstOrDefault(r => r.Id == c.RoomId);
                var slot = room?.Slots.FirstOrDefault(s => s.Id == c.SlotId);
                return slot is null || c.Dates.Count == 0
                    ? null
                    : new Entry(c, room, slot, ToMinutes(c.StartTime), ToMinutes(c.EndTime));
            })
            .Where(e => e is not null)
            .OrderBy(e => e.Slot.Day)
            .ThenBy(e => e.Start)
            .ThenBy(e => e.Room.Name)
            .ToList();

        foreach (var room in entries.GroupBy(e => (e.Slot.Day, e.Room.Id)))
        {
            AssignLanes(room.ToList());
        }

        var days = entries
            .GroupBy(e => e.Slot.Day)
            .Select(day => new Day(
                day.Key,
                day.GroupBy(e => e.Room.Id)
                    .Select(room => new Column(room.First().Room, room.ToList()))
                    .OrderBy(c => rooms.IndexOf(c.Room))
                    .ToList()))
            .ToList();

        var mainVenue = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Room.Venue))
            .GroupBy(e => e.Room.Venue)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var weeks = CoursePlanLogic.Weeks(plan.StartDate, plan.EndDate)
            .SkipWhile(w => !entries.Any(e => HasWeek(e, w)))
            .Reverse()
            .SkipWhile(w => !entries.Any(e => HasWeek(e, w)))
            .Reverse()
            .ToList();

        return new TimeplanModel
        {
            Title = plan.Title,
            MainVenue = mainVenue,
            FirstMinute = entries.Count == 0 ? 17 * 60 : entries.Min(e => e.Start) / 60 * 60,
            LastMinute = entries.Count == 0 ? 22 * 60 : (entries.Max(e => e.End) + 59) / 60 * 60,
            Days = days,
            Weeks = weeks,
            Holidays = weeks.Count == 0
                ? new()
                : HolidaysJson.Parse(plan.HolidaysJson)
                    .Where(h => h.To >= weeks[0] && h.From <= weeks[^1].AddDays(6))
                    .ToList(),
            Entries = entries,
        };
    }
}
