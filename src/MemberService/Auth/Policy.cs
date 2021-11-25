namespace MemberService.Auth
{
    public enum Policy
    {
        IsAdmin,
        IsCoordinator,
        IsInstructor,
        
        CanListEvents,
        CanViewEvent,
        CanEditEvent,

        CanViewMembers
    }
}
