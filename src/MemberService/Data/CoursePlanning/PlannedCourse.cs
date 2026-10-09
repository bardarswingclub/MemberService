namespace MemberService.Data.CoursePlanning;

/// <summary>
/// Ett planlagt kurs i et semester. Kurset er plassert i en sal (RoomId) og en booking av salen (SlotId)
/// som finnes i CoursePlan.RoomsJson. Ikke plasserte kurs har RoomId/SlotId lik null.
/// </summary>
public class PlannedCourse
{
    public Guid Id { get; set; }

    public Guid CoursePlanId { get; set; }

    public CoursePlan CoursePlan { get; set; }

    public string Title { get; set; }

    public string Note { get; set; }

    /// <summary>Beskrivelse av kurset i påmeldingen. Tid og sted legges til automatisk foran.</summary>
    public string Description { get; set; }

    /// <summary>Hjelpetekst i påmeldingen (markdown).</summary>
    public string SignupHelp { get; set; }

    public int Weeks { get; set; }

    public string Color { get; set; }

    public int SortOrder { get; set; }

    public string RoomId { get; set; }

    public string SlotId { get; set; }

    public string StartTime { get; set; }

    public string EndTime { get; set; }

    public List<DateOnly> Dates { get; set; } = new();

    public Guid? EventId { get; set; }

    public Event Event { get; set; }
}
