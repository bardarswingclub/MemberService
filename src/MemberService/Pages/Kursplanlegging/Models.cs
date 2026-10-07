namespace MemberService.Pages.Kursplanlegging;

using MemberService.Data.CoursePlanning;

public class PlanListModel
{
    public record Entry(Guid Id, string Title, DateOnly StartDate, DateOnly EndDate, int Courses, int Imported);

    public IReadOnlyList<Entry> Plans { get; init; }

    public CreatePlanInput Input { get; init; }
}

public class CreatePlanInput
{
    public string Title { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public Guid? CopyFromId { get; set; }

    public bool CopyRooms { get; set; }

    public bool CopyCourses { get; set; }
}

public class HolidaysModel
{
    public CoursePlan Plan { get; init; }

    public IReadOnlyList<Holiday> Holidays { get; init; }

    public IReadOnlyList<Holiday> Suggestions { get; init; }

    public int AffectedCourses { get; init; }
}

public class HolidayInput
{
    public string Name { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public bool Remove { get; set; }

    /// <summary>Forslag tas bare med når de er krysset av.</summary>
    public bool Suggestion { get; set; }

    public bool Include { get; set; }

    public bool Keep => !Remove && (!Suggestion || Include) && From.HasValue;
}

public class CopyCoursesModel
{
    public record Source(string Key, string Title);

    public record Candidate(string Key, string Title, int Weeks, string Details, string Description = null, string SignupHelp = null);

    public CoursePlan Plan { get; init; }

    public IReadOnlyList<Source> Sources { get; init; }

    public string SelectedSource { get; init; }

    public IReadOnlyList<Candidate> Candidates { get; init; }

    public int AlreadyInPlan { get; init; }
}

public class ImportModel
{
    public record SemesterOption(Guid Id, string Title, bool IsActive);

    public record CourseEntry(PlannedCourse Course, string Description, bool Couples);

    public CoursePlan Plan { get; init; }

    public IReadOnlyList<SemesterOption> Semesters { get; init; }

    public IReadOnlyList<CourseEntry> Courses { get; init; }
}
