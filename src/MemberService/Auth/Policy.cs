namespace MemberService.Auth;

public enum Policy
{
    IsAdmin,
    IsCoordinator,
    IsInstructor,

    CanCreateEvent,

    CanListEvents,
    CanViewEvent,
    CanEditEvent,
    CanSetEventSignupStatus,
    CanEditEventSignup,
    CanEditEventOrganizers,

    CanSetPresence,
    CanAddPresenceLesson,

    CanCreateSurvey,
    CanViewSurvey,
    CanEditSurvey,

    CanViewMembers,

    CanViewSemester,
    CanCreateSemester,
}
