namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;

public class Response
{
    public Guid Id { get; set; }

    [Required]
    public Guid SurveyId { get; set; }

    public Survey Survey { get; set; }

    [Required]
    public string UserId { get; set; }

    public User User { get; set; }

    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
}
