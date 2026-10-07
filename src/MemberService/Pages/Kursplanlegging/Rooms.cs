namespace MemberService.Pages.Kursplanlegging;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>En sal vi leier, f.eks. "Storsalen" hos "BLS".</summary>
public record Room
{
    public string Id { get; init; }

    public string Name { get; init; }

    public string Venue { get; init; }

    public string Color { get; init; }

    public List<RoomSlot> Slots { get; init; } = new();
}

/// <summary>En fast leietid for en sal: ukedag (1 = mandag, 7 = søndag), fra/til og datoene den gjelder.</summary>
public record RoomSlot
{
    public string Id { get; init; }

    public int Day { get; init; }

    public string Start { get; init; }

    public string End { get; init; }

    public List<DateOnly> Dates { get; init; } = new();
}

/// <summary>En periode uten kurs, f.eks. "Påske" eller "Vinterferie". Fra og til er med.</summary>
public record Holiday
{
    public string Name { get; init; }

    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public bool Contains(DateOnly date) => date >= From && date <= To;
}

public static class HolidaysJson
{
    public static List<Holiday> Parse(string json)
        => string.IsNullOrWhiteSpace(json)
            ? new()
            : JsonSerializer.Deserialize<List<Holiday>>(json, RoomsJson.Options) ?? new();

    public static string Serialize(IEnumerable<Holiday> holidays)
        => JsonSerializer.Serialize(holidays.OrderBy(h => h.From), RoomsJson.Options);
}

public static class RoomsJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static List<Room> Parse(string json)
        => string.IsNullOrWhiteSpace(json)
            ? new()
            : JsonSerializer.Deserialize<List<Room>>(json, Options) ?? new();

    public static string Serialize(IEnumerable<Room> rooms)
        => JsonSerializer.Serialize(rooms, Options);
}
