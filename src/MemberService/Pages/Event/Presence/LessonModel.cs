namespace MemberService.Pages.Event.Presence;

public class LessonModel
{
    public LessonModel(PresenceModel presence, int lesson)
    {
        Presence = presence;
        Lesson = lesson;
    }

    public PresenceModel Presence { get; }

    public int Lesson { get; }

    public bool HasPrevious => Lesson > 0;

    public bool HasNext => Lesson < Presence.Count - 1;
}
