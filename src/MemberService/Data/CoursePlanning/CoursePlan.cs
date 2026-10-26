namespace MemberService.Data.CoursePlanning;

/// <summary>
/// Planlegging av ett semester. Salene lagres som ett json-dokument per semester (RoomsJson),
/// siden de kun brukes samlet i kursplanleggingen.
/// </summary>
public class CoursePlan
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string RoomsJson { get; set; } = "[]";

    /// <summary>Fridager og ferier uten kurs (json), f.eks. påske og vinterferie.</summary>
    public string HolidaysJson { get; set; } = "[]";

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; }

    public ICollection<PlannedCourse> Courses { get; set; } = new List<PlannedCourse>();
}
