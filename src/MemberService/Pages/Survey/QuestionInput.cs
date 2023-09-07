namespace MemberService.Pages.Survey;

using MemberService.Data.ValueTypes;

public class QuestionInput
{
    public string Title { get; set; }

    public string Description { get; set; }

    public IList<OptionInput> Options { get; set; } = new List<OptionInput>();

    public QuestionType Type { get; set; }

    public class OptionInput
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Action { get; set; }
    }
}
