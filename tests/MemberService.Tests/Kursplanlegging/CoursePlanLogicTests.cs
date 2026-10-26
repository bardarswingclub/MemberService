namespace MemberService.Tests.Kursplanlegging;

using MemberService.Data;
using MemberService.Data.CoursePlanning;
using MemberService.Pages.Kursplanlegging;

using NUnit.Framework;

using Shouldly;

[TestFixture]
public class CoursePlanLogicTests
{
    [Test]
    public void ParseBooking_ReadsBlsConfirmation()
    {
        const string text = """
            Bestilling av Storsalen for Bårdar Swing Club

            Storsalen  27.4.2027 19:00 - 22:30:  Bestilt
            Storsalen  4.5.2027 19:00 - 22:30:  Bestilt
            Storsalen  11.5.2027 19:00 - 22:30:  Bestilt
            Storsalen  18.5.2027 19:00 - 22:30:  Bestilt
            Storsalen  25.5.2027 19:00 - 22:30:  Bestilt
            """;

        var result = CoursePlanLogic.ParseBooking(text);

        var room = result.Rooms.ShouldHaveSingleItem();
        room.Name.ShouldBe("Storsalen");
        result.Skipped.ShouldBeEmpty();
        var slot = room.Slots.ShouldHaveSingleItem();
        slot.Day.ShouldBe(2);
        slot.Start.ShouldBe("19:00");
        slot.End.ShouldBe("22:30");
        slot.Dates.ShouldBe([
            new DateOnly(2027, 4, 27),
            new DateOnly(2027, 5, 4),
            new DateOnly(2027, 5, 11),
            new DateOnly(2027, 5, 18),
            new DateOnly(2027, 5, 25)]);
    }

    [Test]
    public void ParseBooking_GroupsByDayAndTime_AndSkipsNotBooked()
    {
        const string text = """
            Bestilling av Storsalfoaje for Bårdar Swing Club

            Storsalfoaje  27.4.2027 18:30 - 21:00:  Bestilt
            Storsalfoaje  29.4.2027 18:30 - 21:00:  Bestilt
            Storsalfoaje  4.5.2027 18:30 - 21:00:  Avslått
            """;

        var result = CoursePlanLogic.ParseBooking(text);

        var room = result.Rooms.ShouldHaveSingleItem();
        room.Name.ShouldBe("Storsalfoaje");
        room.Slots.Select(s => s.Day).ShouldBe([2, 4]);
        result.Skipped.ShouldHaveSingleItem().ShouldContain("Avslått");
    }

    [Test]
    public void ParseBooking_ReadsSeveralRoomsWithWeekdays()
    {
        const string text = """
            Sal 3  Mandag 20:00 - 21:30
            Sal 5  Mandag 20:00 - 21:30
            Sal 3  onsdag 18:00 - 21:45
            sal 3  Onsdag kl 18.00 - 21.45
            Storsalen  27.4.2027 19:00 - 22:30:  Bestilt
            Dette er ikke en booking
            """;

        var result = CoursePlanLogic.ParseBooking(text);

        result.Rooms.Select(r => r.Name).ShouldBe(["Sal 3", "Sal 5", "Storsalen"]);
        var sal3 = result.Rooms[0].Slots;
        sal3.Select(s => (s.Day, s.Start, s.End)).ShouldBe([(1, "20:00", "21:30"), (3, "18:00", "21:45")]);
        sal3.ShouldAllBe(s => s.Dates.Count == 0);
        result.Rooms[2].Slots.ShouldHaveSingleItem().Dates.ShouldHaveSingleItem();
        result.Skipped.ShouldBe(["Dette er ikke en booking"]);
    }

    [Test]
    public void WeeklyDates_StaysWithinSemester()
    {
        var dates = CoursePlanLogic.WeeklyDates(3, new DateOnly(2027, 1, 7), new DateOnly(2027, 1, 27));

        dates.ShouldBe([new DateOnly(2027, 1, 13), new DateOnly(2027, 1, 20), new DateOnly(2027, 1, 27)]);
    }

    [Test]
    public void CopyRooms_UsesAllWeeksInNewSemester()
    {
        var rooms = new List<Room>
        {
            new() { Id = "r1", Name = "Sal 3", Slots = [new() { Id = "s1", Day = 3, Start = "18:00", End = "22:00", Dates = [new DateOnly(2026, 9, 2)] }] },
        };

        var copy = CoursePlanLogic.CopyRooms(rooms, new DateOnly(2027, 1, 4), new DateOnly(2027, 1, 31));

        copy[0].Slots[0].Id.ShouldBe("s1");
        copy[0].Slots[0].Dates.Count.ShouldBe(4);
        copy[0].Slots[0].Dates.ShouldAllBe(d => d.DayOfWeek == DayOfWeek.Wednesday);
        rooms[0].Slots[0].Dates.Count.ShouldBe(1);
    }

    [Test]
    public void Describe_IncludesDayTimeRoomAndVenue()
    {
        var course = new PlannedCourse { StartTime = "18:00", EndTime = "19:30" };
        var room = new Room { Name = "Sal 3", Venue = "Bårdar Instituttet" };
        var slot = new RoomSlot { Day = 3 };

        CoursePlanLogic.Describe(course, room, slot).ShouldBe("Onsdag kl 18.00-19.30 Sal 3 på Bårdar Instituttet");
    }

    [TestCase("Balboa Basic", true)]
    [TestCase("Solo Jazz 1", false)]
    [TestCase("Intro til solojazz", false)]
    public void ToEvent_UsesCouplesUnlessSoloJazz(string title, bool couples)
    {
        var course = new PlannedCourse
        {
            Title = title,
            Weeks = 6,
            StartTime = "18:00",
            EndTime = "19:30",
            Dates = [new DateOnly(2027, 1, 13), new DateOnly(2027, 1, 20)],
        };

        var semesterId = Guid.NewGuid();
        var entity = course.ToEvent(new Room { Name = "Sal 3" }, new RoomSlot { Day = 3 }, semesterId, new User());

        entity.Title.ShouldBe(title);
        entity.SemesterId.ShouldBe(semesterId);
        entity.Published.ShouldBeFalse();
        entity.SignupOptions.SignupOpensAt.ShouldBeNull();
        entity.SignupOptions.RoleSignup.ShouldBe(couples);
        entity.SignupOptions.AllowPartnerSignup.ShouldBe(couples);
        entity.LessonCount.ShouldBe(2);
    }

    [Test]
    public void Sanitize_RemovesUnsafeValues()
    {
        var rooms = CoursePlanLogic.Sanitize([
            new Room { Name = "Sal", Color = "red;background:url(x)", Slots = [new() { Day = 9 }, new() { Day = 1, Start = "bad", End = "21:00" }] },
        ]);

        rooms[0].Id.ShouldNotBeNullOrEmpty();
        rooms[0].Color.ShouldBeNull();
        var slot = rooms[0].Slots.ShouldHaveSingleItem();
        slot.Start.ShouldBe("18:00");
        slot.End.ShouldBe("21:00");
    }
}

[TestFixture]
public class ExceptExistingTests
{
    [Test]
    public void ExceptExisting_SkipsTitlesAlreadyInPlan_IgnoringCaseAndSpaces()
    {
        var result = CoursePlanLogic.ExceptExisting(
            ["Balboa Basic", "lindy hop  1", "Solo Jazz", "Shag"],
            t => t,
            ["balboa basic", "Lindy Hop 1"]);

        result.ShouldBe(["Solo Jazz", "Shag"]);
    }

    [Test]
    public void ExceptExisting_SkipsDuplicatesAmongCandidates()
    {
        var result = CoursePlanLogic.ExceptExisting(["Shag", "SHAG", "Balboa"], t => t, []);

        result.ShouldBe(["Shag", "Balboa"]);
    }
}

[TestFixture]
public class TimeplanLaneTests
{
    private static TimeplanModel.Entry Entry(string start, string end, int firstWeek, int weeks)
    {
        var course = new PlannedCourse
        {
            StartTime = start,
            EndTime = end,
            Dates = Enumerable.Range(firstWeek, weeks).Select(w => new DateOnly(2027, 1, 6).AddDays(7 * w)).ToList(),
        };
        return new TimeplanModel.Entry(course, new Room(), new RoomSlot(), CoursePlanLogic.ToMinutes(start), CoursePlanLogic.ToMinutes(end));
    }

    [Test]
    public void SequentialSixWeekCoursesAtSameTime_GetSeparateLanes()
    {
        var first = Entry("19:00", "20:15", 0, 6);
        var firstLater = Entry("20:15", "21:45", 0, 6);
        var second = Entry("19:00", "20:30", 6, 6);
        var secondLater = Entry("20:30", "21:45", 6, 6);

        TimeplanModel.AssignLanes([second, first, firstLater, secondLater]);

        (first.Lane, firstLater.Lane, second.Lane, secondLater.Lane).ShouldBe((0, 0, 1, 1));
        first.LaneCount.ShouldBe(2);
    }

    [Test]
    public void CoursesAtDifferentTimes_UseFullWidth()
    {
        var early = Entry("18:00", "19:30", 0, 6);
        var late = Entry("19:30", "21:00", 6, 6);

        TimeplanModel.AssignLanes([early, late]);

        early.LaneCount.ShouldBe(1);
        late.LaneCount.ShouldBe(1);
    }
}

[TestFixture]
public class HolidayTests
{
    [TestCase(2026, 4, 5)]
    [TestCase(2027, 3, 28)]
    [TestCase(2028, 4, 16)]
    public void EasterSunday(int year, int month, int day)
        => CoursePlanLogic.EasterSunday(year).ShouldBe(new DateOnly(year, month, day));

    [Test]
    public void SuggestHolidays_Spring2027()
    {
        var holidays = CoursePlanLogic.SuggestHolidays(new DateOnly(2027, 1, 4), new DateOnly(2027, 6, 15));

        holidays.ShouldContain(new Holiday { Name = "Vinterferie", From = new(2027, 2, 22), To = new(2027, 2, 28) });
        holidays.ShouldContain(new Holiday { Name = "Påske", From = new(2027, 3, 25), To = new(2027, 3, 29) });
        holidays.ShouldContain(new Holiday { Name = "Kristi himmelfartsdag", From = new(2027, 5, 6), To = new(2027, 5, 6) });
        holidays.ShouldNotContain(h => h.Name == "Høstferie" || h.Name == "Nyttårsdag");
    }

    [Test]
    public void SuggestHolidays_Autumn2026_HasOsloAutumnBreak()
    {
        var holidays = CoursePlanLogic.SuggestHolidays(new DateOnly(2026, 8, 15), new DateOnly(2026, 12, 15));

        holidays.ShouldBe([new Holiday { Name = "Høstferie", From = new(2026, 9, 28), To = new(2026, 10, 4) }]);
    }

    [Test]
    public void MoveOffHolidays_PostponesEasterThursdayOneWeek()
    {
        // Torsdagskurs i 6 uker fra 4. mars 2027; skjærtorsdag 25. mars er fri
        var slotDates = Enumerable.Range(0, 12).Select(i => new DateOnly(2027, 3, 4).AddDays(7 * i)).ToList();
        var course = slotDates.Take(6).ToList();
        var holidays = CoursePlanLogic.SuggestHolidays(new DateOnly(2027, 1, 4), new DateOnly(2027, 6, 15));

        var moved = CoursePlanLogic.MoveOffHolidays(course, slotDates, holidays);

        moved.Count.ShouldBe(6);
        moved.ShouldNotContain(new DateOnly(2027, 3, 25));
        moved[^1].ShouldBe(new DateOnly(2027, 4, 15));
    }

    [Test]
    public void MoveOffHolidays_LeavesCoursesWithoutHolidaysAlone()
    {
        var dates = new List<DateOnly> { new(2027, 1, 13), new(2027, 1, 20) };

        CoursePlanLogic.MoveOffHolidays(dates, dates, []).ShouldBe(dates);
    }
}

[TestFixture]
public class DescriptionTests
{
    [Test]
    public void ToEvent_PutsDescriptionFirst_ThenSchedule_OnOneLine_AndSetsHelp()
    {
        var course = new PlannedCourse
        {
            Title = "Balboa Basic",
            StartTime = "18:00",
            EndTime = "19:30",
            Description = "Nybegynner nivå",
            SignupHelp = "Meld dere gjerne på som par.",
            Dates = [new DateOnly(2027, 1, 13)],
        };

        var entity = course.ToEvent(new Room { Name = "Sal 3", Venue = "Bårdar Instituttet" }, new RoomSlot { Day = 3 }, Guid.NewGuid(), new User());

        entity.Description.ShouldBe("Nybegynner nivå. Onsdag kl 18.00-19.30 Sal 3 på Bårdar Instituttet");
        entity.SignupOptions.SignupHelp.ShouldBe("Meld dere gjerne på som par.");
    }

    [TestCase("Nybegynner nivå.", "Nybegynner nivå. Tirsdag kl 19.00-20.30 Sal 7")]
    [TestCase("Linje en\nlinje to", "Linje en linje to. Tirsdag kl 19.00-20.30 Sal 7")]
    [TestCase(null, "Tirsdag kl 19.00-20.30 Sal 7")]
    public void CombineDescription(string description, string expected)
        => CoursePlanLogic.CombineDescription("Tirsdag kl 19.00-20.30 Sal 7", description).ShouldBe(expected);

    [TestCase("Nybegynner nivå. Onsdag kl 18.00-19.30 (sosialdans 19.30-20.00) Sal 3 på Bårdar Instituttet", "Nybegynner nivå.")]
    [TestCase("Onsdag kl 18.00-19.30 Sal 3 på Bårdar Instituttet", null)]
    [TestCase("Tirsdager kl. 19:00 i Sal 1", null)]
    [TestCase("Grunnkurs i balboa for alle", "Grunnkurs i balboa for alle")]
    [TestCase(null, null)]
    public void StripSchedule_RemovesTimeAndPlace(string input, string expected)
        => CoursePlanLogic.StripSchedule(input).ShouldBe(expected);
}

[TestFixture]
public class SharedHelperTests
{
    [TestCase("Solo Jazz 1", true)]
    [TestCase("Solojazz for viderekomne", true)]
    [TestCase("Balboa Basic", false)]
    [TestCase(null, false)]
    public void IsSoloJazzTitle(string title, bool expected) => title.IsSoloJazzTitle().ShouldBe(expected);

    [Test]
    public void Report_ClassifiesSolojazzAsSoloJazz()
        => MemberService.Pages.Reports.MultiClassModel.ClassifyTitle("Solojazz 1").ShouldBe(MemberService.Pages.Reports.MultiClassModel.DanceStyle.SoloJazz);

    [TestCase(2026, 10, "Våren 2027")]
    [TestCase(2027, 3, "Høsten 2027")]
    public void NextSemesterTitle(int year, int month, string expected)
        => new DateTime(year, month, 15).GetStartOfNextSemester().GetSemesterTitle().ShouldBe(expected);

    [Test]
    public void FindPlacement_ReturnsRoomAndSlot_OrNothing()
    {
        var rooms = new List<Room> { new() { Id = "r1", Name = "Sal 3", Slots = [new() { Id = "s1", Day = 3 }] } };

        rooms.FindPlacement(new PlannedCourse { RoomId = "r1", SlotId = "s1" }).Slot.Id.ShouldBe("s1");
        rooms.FindPlacement(new PlannedCourse { RoomId = "r1", SlotId = "gone" }).ShouldBe((null, null));
        rooms.FindPlacement(new PlannedCourse()).ShouldBe((null, null));
    }

    [Test]
    public void EventDescription_IsNullWhenNotPlaced()
    {
        var course = new PlannedCourse { Description = "Nybegynner", StartTime = "18:00", EndTime = "19:30" };

        CoursePlanLogic.EventDescription(course, null, null).ShouldBeNull();
        CoursePlanLogic.EventDescription(course, new Room { Name = "Sal 3", Venue = " " }, new RoomSlot { Day = 3 })
            .ShouldBe("Nybegynner. Onsdag kl 18.00-19.30 Sal 3");
    }

    [Test]
    public void Sanitize_TrimsRoomNameAndVenue()
    {
        var room = CoursePlanLogic.Sanitize([new Room { Name = "  Sal 3 ", Venue = "   " }]).Single();

        room.Name.ShouldBe("Sal 3");
        room.Venue.ShouldBeNull();
    }

    [TestCase("", 2027, 1, 4, 2027, 6, 15, false)]
    [TestCase("Våren 2027", 2027, 6, 15, 2027, 1, 4, false)]
    [TestCase("Våren 2027", 2027, 1, 4, 2027, 6, 15, true)]
    public void CreatePlanInput_Validate(string title, int y1, int m1, int d1, int y2, int m2, int d2, bool valid)
        => (new CreatePlanInput { Title = title, StartDate = new(y1, m1, d1), EndDate = new(y2, m2, d2) }.Validate() is null).ShouldBe(valid);

    [Test]
    public void ImportedCourse_GetsSameCreatorRightsAsOtherCourses()
    {
        var user = new User();
        var imported = new PlannedCourse { Title = "Shag" }.ToEvent(null, null, Guid.NewGuid(), user).Organizers.Single();
        var expected = MemberService.Pages.Event.Logic.CreatorOrganizer(user);

        (imported.CanEdit, imported.CanEditOrganizers, imported.CanSetSignupStatus, imported.CanSetPresence, imported.CanAddPresenceLesson, imported.CanEditSignup)
            .ShouldBe((expected.CanEdit, expected.CanEditOrganizers, expected.CanSetSignupStatus, expected.CanSetPresence, expected.CanAddPresenceLesson, expected.CanEditSignup));
    }
}
