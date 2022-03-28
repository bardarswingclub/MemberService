namespace MemberService.Pages.Home;

using Clave.Expressionify;

public partial class SurveyModel
{
    public string Title { get; set; }

    public Guid SurveyId { get; set; }

    [Expressionify]
    public static SurveyModel Create(Data.Semester s) =>
        new()
        {
            Title = s.Title,
            SurveyId = s.Survey.Id,
        };
}
