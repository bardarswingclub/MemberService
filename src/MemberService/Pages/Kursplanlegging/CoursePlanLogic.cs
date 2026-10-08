namespace MemberService.Pages.Kursplanlegging;

using System.Globalization;
using System.Text.RegularExpressions;

using MemberService.Data;
using MemberService.Data.CoursePlanning;
using MemberService.Data.ValueTypes;

public static partial class CoursePlanLogic
{
    public static readonly string[] DayNames = ["", "Mandag", "Tirsdag", "Onsdag", "Torsdag", "Fredag", "Lørdag", "Søndag"];

    public static int ToDay(this DateOnly date) => date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;

    public static DateOnly StartOfWeek(this DateOnly date) => date.AddDays(1 - date.ToDay());

    public static int IsoWeek(this DateOnly date) => ISOWeek.GetWeekOfYear(date.ToDateTime(TimeOnly.MinValue));

    /// <summary>Mandagen i hver uke i semesteret.</summary>
    public static List<DateOnly> Weeks(DateOnly start, DateOnly end)
    {
        var result = new List<DateOnly>();
        for (var d = start.StartOfWeek(); d <= end; d = d.AddDays(7))
        {
            result.Add(d);
        }
        return result;
    }

    /// <summary>Alle datoer med gitt ukedag innenfor semesteret.</summary>
    public static List<DateOnly> WeeklyDates(int day, DateOnly start, DateOnly end)
        => Weeks(start, end)
            .Select(w => w.AddDays(day - 1))
            .Where(d => d >= start && d <= end)
            .ToList();

    public static int ToMinutes(string time)
    {
        var parts = (time ?? "").Split(':', '.');
        return parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var hours)
            && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var minutes)
            ? hours * 60 + minutes
            : 0;
    }

    [GeneratedRegex(@"^\d{2}:\d{2}$")]
    private static partial Regex TimeRegex();

    [GeneratedRegex(@"^#[0-9a-fA-F]{6}$")]
    private static partial Regex ColorRegex();

    public static string SafeTime(string time) => time is not null && TimeRegex().IsMatch(time) ? time : null;

    public static string SafeColor(string color) => color is not null && ColorRegex().IsMatch(color) ? color : null;

    public static List<Room> Sanitize(IEnumerable<Room> rooms)
        => rooms
            .Where(r => r is not null)
            .Select(r => r with
            {
                Id = string.IsNullOrWhiteSpace(r.Id) ? Guid.NewGuid().ToString("N")[..8] : r.Id,
                Color = SafeColor(r.Color),
                Slots = (r.Slots ?? new())
                    .Where(s => s is not null && s.Day is >= 1 and <= 7)
                    .Select(s => s with
                    {
                        Id = string.IsNullOrWhiteSpace(s.Id) ? Guid.NewGuid().ToString("N")[..8] : s.Id,
                        Start = SafeTime(s.Start) ?? "18:00",
                        End = SafeTime(s.End) ?? "22:00",
                        Dates = (s.Dates ?? new()).Distinct().OrderBy(d => d).ToList(),
                    })
                    .ToList(),
            })
            .ToList();

    public static string FromMinutes(int minutes) => $"{minutes / 60:00}:{minutes % 60:00}";

    public static readonly string[] Colors = ["#dbe7f3", "#fde2c8", "#d5f0d8", "#f8d7e3", "#e6dcf5", "#fff3bf", "#cdeeee", "#e9ecef"];

    /// <summary>Navn sammenlignes uten forskjell på store/små bokstaver og ekstra mellomrom.</summary>
    public static string NormalizeTitle(string title)
        => string.Join(' ', (title ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();

    /// <summary>
    /// Fjerner kurs som allerede finnes i planen (samme navn), og duplikater blant kandidatene,
    /// slik at importen kan kjøres flere ganger.
    /// </summary>
    public static List<T> ExceptExisting<T>(IEnumerable<T> candidates, Func<T, string> title, IEnumerable<string> existingTitles)
    {
        var seen = existingTitles.Select(NormalizeTitle).ToHashSet();
        return candidates.Where(c => seen.Add(NormalizeTitle(title(c)))).ToList();
    }

    public static bool IsSoloJazz(string title)
        => title?.Contains("solo jazz", StringComparison.OrdinalIgnoreCase) == true
        || title?.Contains("solojazz", StringComparison.OrdinalIgnoreCase) == true;

    /// <summary>F.eks. "Onsdag kl 18.00-19.30 Sal 3 på Bårdar Instituttet".</summary>
    public static string Describe(PlannedCourse course, Room room, RoomSlot slot)
    {
        if (room is null || slot is null) return null;

        var place = string.IsNullOrWhiteSpace(room.Venue) ? room.Name : $"{room.Name} på {room.Venue}";
        return $"{DayNames[slot.Day]} kl {course.StartTime?.Replace(':', '.')}-{course.EndTime?.Replace(':', '.')} {place}";
    }

    /// <summary>
    /// Beskrivelsen er én linje i påmeldingen: kort beskrivelse først og så tid og sted, slik som
    /// "Nybegynner nivå. Onsdag kl 18.00-19.30 Sal 3 på Bårdar Instituttet".
    /// </summary>
    public static string CombineDescription(string schedule, string description)
    {
        description = SingleLine(description);
        if (string.IsNullOrEmpty(schedule)) return description;
        if (string.IsNullOrEmpty(description)) return schedule;
        var end = description[^1];
        return end is '.' or '!' or '?' or ':' ? $"{description} {schedule}" : $"{description}. {schedule}";
    }

    public static string SingleLine(string text)
    {
        var result = string.Join(' ', (text ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        return result.Length == 0 ? null : result;
    }

    [GeneratedRegex(@"\b(mandag|tirsdag|onsdag|torsdag|fredag|lørdag|søndag)(er)?\s+kl\.?\s*\d{1,2}[.:]\d{2}.*$", RegexOptions.IgnoreCase)]
    private static partial Regex ScheduleRegex();

    /// <summary>
    /// Fjerner tid og sted fra en beskrivelse fra et tidligere semester ("Nybegynner nivå. Onsdag kl 18.00-19.30 Sal 3 ..."),
    /// siden tid og sted legges til på nytt fra planen når kurset opprettes.
    /// </summary>
    public static string StripSchedule(string description)
    {
        var line = SingleLine(description);
        if (line is null) return null;

        return SingleLine(ScheduleRegex().Replace(line, ""));
    }

    public static Event ToEvent(this PlannedCourse course, Room room, RoomSlot slot, Guid semesterId, User user)
    {
        var couples = !IsSoloJazz(course.Title);
        var dates = course.Dates.OrderBy(d => d).ToList();
        return new Event
        {
            Title = course.Title,
            Description = CombineDescription(Describe(course, room, slot), course.Description),
            Type = EventType.Class,
            SemesterId = semesterId,
            CreatedAt = TimeProvider.UtcNow,
            CreatedByUser = user,
            LessonCount = dates.Count > 0 ? dates.Count : course.Weeks,
            Published = false,
            SignupOptions = new()
            {
                SignupOpensAt = null,
                SignupClosesAt = null,
                SignupHelp = string.IsNullOrWhiteSpace(course.SignupHelp) ? null : course.SignupHelp.Trim(),
                RoleSignup = couples,
                AllowPartnerSignup = couples,
            },
            Organizers =
            {
                new()
                {
                    User = user,
                    UpdatedByUser = user,
                    UpdatedAt = TimeProvider.UtcNow,
                    CanEdit = true,
                    CanEditOrganizers = true,
                    CanSetSignupStatus = true,
                    CanSetPresence = true,
                    CanAddPresenceLesson = true
                }
            }
        };
    }

    /// <summary>
    /// Lager nye saler for et nytt semester basert på salene fra et tidligere semester.
    /// Vi får normalt beholde samme ukedager og tider, så alle bookinger gjelder alle uker i det nye semesteret.
    /// </summary>
    public static List<Room> CopyRooms(IEnumerable<Room> rooms, DateOnly start, DateOnly end)
        => rooms
            .Select(r => r with
            {
                Slots = r.Slots
                    .Select(s => s with { Dates = WeeklyDates(s.Day, start, end) })
                    .ToList()
            })
            .ToList();

    public static bool IsClosed(this IEnumerable<Holiday> holidays, DateOnly date) => holidays.Any(h => h.Contains(date));

    /// <summary>Første søndag etter første fullmåne etter vårjevndøgn (gregoriansk, anonym algoritme).</summary>
    public static DateOnly EasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }

    /// <summary>
    /// Forslag til fridager i semesteret: norske helligdager, samt vinterferie (uke 8) og høstferie (uke 40) i Oslo.
    /// Påsken foreslås fra skjærtorsdag til 2. påskedag. Forslagene kan endres på siden for fridager.
    /// </summary>
    public static List<Holiday> SuggestHolidays(DateOnly start, DateOnly end)
    {
        var result = new List<Holiday>();

        for (var year = start.Year; year <= end.Year; year++)
        {
            var easter = EasterSunday(year);
            DateOnly Week(int week) => DateOnly.FromDateTime(ISOWeek.ToDateTime(year, week, DayOfWeek.Monday));

            result.Add(new() { Name = "Nyttårsdag", From = new(year, 1, 1), To = new(year, 1, 1) });
            result.Add(new() { Name = "Vinterferie", From = Week(8), To = Week(8).AddDays(6) });
            result.Add(new() { Name = "Påske", From = easter.AddDays(-3), To = easter.AddDays(1) });
            result.Add(new() { Name = "1. mai", From = new(year, 5, 1), To = new(year, 5, 1) });
            result.Add(new() { Name = "17. mai", From = new(year, 5, 17), To = new(year, 5, 17) });
            result.Add(new() { Name = "Kristi himmelfartsdag", From = easter.AddDays(39), To = easter.AddDays(39) });
            result.Add(new() { Name = "Pinse", From = easter.AddDays(49), To = easter.AddDays(50) });
            result.Add(new() { Name = "Høstferie", From = Week(40), To = Week(40).AddDays(6) });
            result.Add(new() { Name = "Jul", From = new(year, 12, 24), To = new(year, 12, 26) });
        }

        return result
            .Where(h => h.To >= start && h.From <= end)
            .OrderBy(h => h.From)
            .ToList();
    }

    /// <summary>
    /// Flytter kursdatoer som faller på fridager: datoen fjernes, og kurset får i stedet
    /// neste ledige dato i salen etter den siste kursdatoen (typisk uka etter).
    /// </summary>
    public static List<DateOnly> MoveOffHolidays(IReadOnlyCollection<DateOnly> dates, IEnumerable<DateOnly> slotDates, IReadOnlyList<Holiday> holidays)
    {
        var kept = dates.Where(d => !holidays.IsClosed(d)).OrderBy(d => d).ToList();
        var missing = dates.Count - kept.Count;
        if (missing == 0 || dates.Count == 0) return kept;

        var after = dates.Max();
        kept.AddRange(slotDates
            .Where(d => d > after && !holidays.IsClosed(d) && !kept.Contains(d))
            .OrderBy(d => d)
            .Take(missing));

        return kept;
    }

    public static PlannedCourse CopyCourse(PlannedCourse course, List<Room> rooms, IReadOnlyList<Holiday> holidays)
    {
        var slot = rooms.FirstOrDefault(r => r.Id == course.RoomId)?.Slots.FirstOrDefault(s => s.Id == course.SlotId);
        return new PlannedCourse
        {
            Title = course.Title,
            Note = course.Note,
            Description = course.Description,
            SignupHelp = course.SignupHelp,
            Weeks = course.Weeks,
            Color = course.Color,
            SortOrder = course.SortOrder,
            RoomId = slot is null ? null : course.RoomId,
            SlotId = slot?.Id,
            StartTime = slot is null ? null : course.StartTime,
            EndTime = slot is null ? null : course.EndTime,
            Dates = slot?.Dates.Where(d => !holidays.IsClosed(d)).OrderBy(d => d).Take(course.Weeks).ToList() ?? new(),
        };
    }

    public record ParsedRoom(string Name, List<RoomSlot> Slots);

    public record ParsedBooking(List<ParsedRoom> Rooms, List<string> Skipped);

    [GeneratedRegex(@"^\s*Bestilling av (?<room>.+?) for ", RegexOptions.IgnoreCase)]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"^\s*(?<room>.*?)\s*(?<d>\d{1,2})\.(?<m>\d{1,2})\.(?<y>\d{4})\s+(?<from>\d{1,2}[:.]\d{2})\s*-\s*(?<to>\d{1,2}[:.]\d{2})\s*:?\s*(?<status>.*)$")]
    private static partial Regex DateLineRegex();

    [GeneratedRegex(@"^\s*(?<room>.*?)\s*\b(?<day>mandag|tirsdag|onsdag|torsdag|fredag|lørdag|søndag)(er)?\s+(kl\.?\s*)?(?<from>\d{1,2}[:.]\d{2})\s*-\s*(?<to>\d{1,2}[:.]\d{2})\s*:?\s*(?<status>.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex WeekdayLineRegex();

    /// <summary>
    /// Leser bookinger av en eller flere saler. Hver linje er enten en bestilt dato, f.eks. fra BLS:
    /// <code>
    /// Bestilling av Storsalen for Bårdar Swing Club
    ///
    /// Storsalen  27.4.2027 19:00 - 22:30:  Bestilt
    /// </code>
    /// eller en fast ukedag som gjelder alle uker i semesteret (dette er vanlig for salene på Bårdar):
    /// <code>
    /// Sal 3  Mandag 20:00 - 21:30
    /// </code>
    /// Linjene grupperes på sal, ukedag og klokkeslett, slik at hver gruppe blir en booking av salen.
    /// Bookinger på ukedag har ingen datoer; de fylles med alle ukene i semesteret når de legges inn.
    /// </summary>
    public static ParsedBooking ParseBooking(string text)
    {
        string headerRoom = null;
        var entries = new List<(string Room, int Day, DateOnly? Date, string Start, string End)>();
        var skipped = new List<string>();

        foreach (var line in (text ?? "").Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0))
        {
            if (line.Length > 300)
            {
                skipped.Add(line[..100] + "…");
                continue;
            }

            var header = HeaderRegex().Match(line);
            if (header.Success)
            {
                headerRoom = header.Groups["room"].Value.Trim();
                continue;
            }

            var dateLine = DateLineRegex().Match(line);
            var weekdayLine = dateLine.Success ? Match.Empty : WeekdayLineRegex().Match(line);
            var match = dateLine.Success ? dateLine : weekdayLine;

            if (!match.Success)
            {
                skipped.Add(line);
                continue;
            }

            var status = match.Groups["status"].Value.Trim();
            if (status.Length > 0 && !status.StartsWith("Bestilt", StringComparison.OrdinalIgnoreCase))
            {
                skipped.Add(line);
                continue;
            }

            var room = match.Groups["room"].Value.Trim() is { Length: > 0 } name ? name : headerRoom;
            if (room is null)
            {
                skipped.Add(line);
                continue;
            }

            DateOnly? date = null;
            int day;
            if (dateLine.Success)
            {
                date = new DateOnly(
                    int.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups["m"].Value, CultureInfo.InvariantCulture),
                    int.Parse(match.Groups["d"].Value, CultureInfo.InvariantCulture));
                day = date.Value.ToDay();
            }
            else
            {
                day = Array.FindIndex(DayNames, d => d.Equals(match.Groups["day"].Value, StringComparison.OrdinalIgnoreCase));
            }

            entries.Add((
                room,
                day,
                date,
                FromMinutes(ToMinutes(match.Groups["from"].Value)),
                FromMinutes(ToMinutes(match.Groups["to"].Value))));
        }

        var rooms = entries
            .GroupBy(e => e.Room, StringComparer.OrdinalIgnoreCase)
            .Select(room => new ParsedRoom(
                room.First().Room,
                room.GroupBy(e => (e.Day, e.Start, e.End))
                    .OrderBy(g => g.Key.Day)
                    .ThenBy(g => g.Key.Start)
                    .Select(g => new RoomSlot
                    {
                        Id = Guid.NewGuid().ToString("N")[..8],
                        Day = g.Key.Day,
                        Start = g.Key.Start,
                        End = g.Key.End,
                        Dates = g.Where(e => e.Date.HasValue).Select(e => e.Date.Value).Distinct().OrderBy(d => d).ToList(),
                    })
                    .ToList()))
            .ToList();

        return new ParsedBooking(rooms, skipped);
    }
}
