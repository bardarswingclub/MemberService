namespace MemberService.Tests.Kursplanlegging;

using System.Reflection;
using System.Security.Claims;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Auth.Requirements;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Kursplanlegging;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NUnit.Framework;

using Shouldly;

/// <summary>
/// Instruktørkoordinatorer skal kunne planlegge kurs og gjøre alt med kurs i aktive semestre,
/// men ikke endre semesterroller, avgiftsfritak, betalinger eller kurs i tidligere semestre.
/// </summary>
[TestFixture]
public class AuthorizationTests
{
    private static readonly DateTime Now = new(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc);

    private IDisposable _time;
    private MemberContext _database;
    private Guid _activeClass;
    private Guid _oldClass;
    private Guid _party;
    private Guid _semesterWorkshop;
    private Guid _activeSignup;
    private Guid _oldSignup;

    [SetUp]
    public async Task SetUp()
    {
        _time = TemporaryTime.Is(Now);
        _database = new MemberContext(new DbContextOptionsBuilder<MemberContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseExpressionify(o => o.WithEvaluationMode(ExpressionEvaluationMode.FullCompatibilityButSlow))
            .Options);

        var active = new Semester { Id = Guid.NewGuid(), Title = "Høsten 2026", SignupOpensAt = new DateTime(2026, 8, 1) };
        var old = new Semester { Id = Guid.NewGuid(), Title = "Våren 2026", SignupOpensAt = new DateTime(2026, 1, 5) };
        _database.Semesters.AddRange(active, old);

        Event Course(string title, Guid? semesterId) => new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = Now,
            SemesterId = semesterId,
            Type = semesterId.HasValue ? EventType.Class : EventType.Party,
        };

        var activeClass = Course("Balboa Basic", active.Id);
        var oldClass = Course("Lindy Hop 1", old.Id);
        var party = Course("Julebord", null);
        var semesterWorkshop = Course("Workshop i semesteret", active.Id);
        semesterWorkshop.Type = EventType.Workshop;
        _database.Events.AddRange(activeClass, oldClass, party, semesterWorkshop);

        var member = new User { Id = "member-1", Email = "medlem@example.com", FullName = "Medlem" };
        _database.Users.Add(member);
        var activeSignup = new EventSignup { Id = Guid.NewGuid(), EventId = activeClass.Id, UserId = member.Id, SignedUpAt = Now };
        var oldSignup = new EventSignup { Id = Guid.NewGuid(), EventId = oldClass.Id, UserId = member.Id, SignedUpAt = Now };
        _database.EventSignups.AddRange(activeSignup, oldSignup);
        await _database.SaveChangesAsync();

        (_activeClass, _oldClass, _party, _semesterWorkshop) = (activeClass.Id, oldClass.Id, party.Id, semesterWorkshop.Id);
        (_activeSignup, _oldSignup) = (activeSignup.Id, oldSignup.Id);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
        _time.Dispose();
    }

    private static ClaimsPrincipal UserInRoles(params string[] roles)
        => new(new ClaimsIdentity(
            roles.Select(r => new Claim(ClaimTypes.Role, r)).Append(new Claim(ClaimTypes.NameIdentifier, "user-1")),
            "test"));

    private async Task<bool> IsAuthorized(ClaimsPrincipal user, Policy policy, Guid? resource = null)
    {
        var requirement = new Requirement(policy);
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await new RoleRequirementsHandler().HandleAsync(context);
        await new SemesterRequirementsHandler(_database).HandleAsync(context);
        await new EventRequirementsHandler(_database).HandleAsync(context);

        return context.HasSucceeded;
    }

    private static ClaimsPrincipal Coordinator => UserInRoles(Roles.INSTRUKTORKOORDINATOR);

    [TestCase(Policy.CanPlanCourses)]
    [TestCase(Policy.CanViewSemester)]
    [TestCase(Policy.CanCreateSemesterEvent)]
    public async Task Coordinator_CanPlanAndCreateCourses(Policy policy)
        => (await IsAuthorized(Coordinator, policy)).ShouldBeTrue();

    [TestCase(Policy.CanEditSemesterRoles)]
    [TestCase(Policy.CanToggleUserFeeExemption)]
    [TestCase(Policy.CanToggleRoles)]
    [TestCase(Policy.CanAddManualPayment)]
    [TestCase(Policy.CanUpdatePayments)]
    [TestCase(Policy.CanListPayments)]
    public async Task Coordinator_CannotChangeRolesFeesOrPayments(Policy policy)
        => (await IsAuthorized(Coordinator, policy)).ShouldBeFalse();

    [TestCase(Policy.CanViewEvent)]
    [TestCase(Policy.CanEditEvent)]
    [TestCase(Policy.CanSetEventSignupStatus)]
    [TestCase(Policy.CanSendEventEmail)]
    [TestCase(Policy.CanEditEventSignup)]
    [TestCase(Policy.CanEditEventOrganizers)]
    [TestCase(Policy.CanSetPresence)]
    [TestCase(Policy.CanAddPresenceLesson)]
    public async Task Coordinator_CanDoEverythingWithClassesInActiveSemester(Policy policy)
        => (await IsAuthorized(Coordinator, policy, _activeClass)).ShouldBeTrue();

    [Test]
    public async Task Coordinator_CanEditSignupsOnClassesInActiveSemester_BySignupId()
    {
        (await IsAuthorized(Coordinator, Policy.CanEditEventSignup, _activeSignup)).ShouldBeTrue();
        (await IsAuthorized(Coordinator, Policy.CanEditEventSignup, _oldSignup)).ShouldBeFalse();
    }

    [TestCase(Policy.CanViewEvent)]
    [TestCase(Policy.CanEditEvent)]
    [TestCase(Policy.CanSetEventSignupStatus)]
    [TestCase(Policy.CanEditEventSignup)]
    [TestCase(Policy.CanEditEventOrganizers)]
    public async Task Coordinator_CannotAdministerClassesInOldSemesters(Policy policy)
        => (await IsAuthorized(Coordinator, policy, _oldClass)).ShouldBeFalse();

    [TestCase(Policy.CanViewEvent)]
    [TestCase(Policy.CanEditEvent)]
    [TestCase(Policy.CanEditEventOrganizers)]
    public async Task Coordinator_CannotAdministerOtherEvents(Policy policy)
    {
        (await IsAuthorized(Coordinator, policy, _party)).ShouldBeFalse();
        (await IsAuthorized(Coordinator, policy, _semesterWorkshop)).ShouldBeFalse();
    }

    [Test]
    public async Task OrdinaryMember_CannotPlanOrEditClasses()
    {
        var member = UserInRoles();

        (await IsAuthorized(member, Policy.CanPlanCourses)).ShouldBeFalse();
        (await IsAuthorized(member, Policy.CanEditEvent, _activeClass)).ShouldBeFalse();
    }

    [Test]
    public async Task Board_CanPlanCourses()
        => (await IsAuthorized(UserInRoles(Roles.STYRET), Policy.CanPlanCourses)).ShouldBeTrue();

    [Test]
    public void AllChangingActions_ValidateAntiforgeryToken()
    {
        var actions = new[] { typeof(KursplanleggingController), typeof(CoursePlanApiController) }
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttributes().Any(a => a is HttpPostAttribute or HttpPutAttribute or HttpDeleteAttribute))
            .ToList();

        actions.ShouldNotBeEmpty();
        actions
            .Where(m => m.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() is null)
            .Select(m => $"{m.DeclaringType.Name}.{m.Name}")
            .ShouldBeEmpty();
    }

    [Test]
    public void AllActions_RequireCanPlanCourses()
    {
        foreach (var controller in new[] { typeof(KursplanleggingController), typeof(CoursePlanApiController) })
        {
            controller.GetCustomAttribute<AuthorizeAttribute>().ShouldNotBeNull().Policy.ShouldBe(nameof(Policy.CanPlanCourses));
        }
    }
}
