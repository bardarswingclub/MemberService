using Clave.Expressionify;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event.Survey
{
    public class AnswerModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Status Status { get; set; }

        [Expressionify]
        public static AnswerModel Create(Data.EventSignup s) =>
            new AnswerModel
            {
                Id = s.UserId,
                Name = s.User.FullName,
                Status = s.Status
            };
    }
}