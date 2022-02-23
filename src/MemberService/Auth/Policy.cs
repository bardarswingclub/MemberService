namespace MemberService.Auth;

public enum Policy
{
    IsAdmin,

    CanToggleRoles,
    CanToggleUserFeeExemption,

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
    CanViewOlderMembers,
    CanSendEmailToMembers,
    CanAddManualPayment,
    CanUpdatePayments,
    CanSeeStripeLink,

    CanViewSemester,
    CanCreateSemester,
    CanCreateSemesterEvent,
    CanEditSemester,
    CanEditSemesterRoles,
    CanPreviewSemesterSignup,

    CanCreateAnnualMeeting,
    CanEditAnnualMeeting,
    CanViewAnnualMeetingAttendees
}
