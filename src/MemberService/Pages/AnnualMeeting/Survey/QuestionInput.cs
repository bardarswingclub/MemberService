namespace MemberService.Pages.AnnualMeeting.Survey;




public class QuestionInput
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string From { get; set; }

    public string Until { get; set; }

    public IList<OptionInput> Options { get; set; } = new List<OptionInput>();

    public class OptionInput
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Action { get; set; }
    }
}
