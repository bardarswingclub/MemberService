namespace MemberService.Pages.AnnualMeeting.Survey;





using Clave.Expressionify;

public partial class SurveyResultModel
{
    public Guid Id { get; set; }

    public Guid MeetingId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public IReadOnlyList<QuestionModel> Questions { get; set; }

    public IReadOnlyCollection<ResponseModel> Responses { get; set; }

    [Expressionify]
    public static SurveyResultModel Create(Data.AnnualMeeting s) =>
        new()
        {
            Id = s.Survey.Id,
            MeetingId = s.Id,
            Title = s.Survey.Title,
            Description = s.Survey.Description,
            Questions = s.Survey.Questions
                .OrderBy(s => s.Order)
                .Select(q => QuestionModel.Create(s.Id, q))
                .ToList(),
            Responses = s.Survey.Responses.AsQueryable()
                .SelectMany(r => r.Answers.Select(a => ResponseModel.Create(r, a)))
                .ToList()
        };

}
