using System;

using Clave.Expressionify;

using MemberService.Data;

namespace MemberService.Pages.AnnualMeeting.Survey
{
    public partial class ResponseModel
    {
        public string Name { get; set; }

        public string UserId { get; set; }

        public Guid OptionId { get; set; }

        [Expressionify]
        public static ResponseModel Create(Response r, QuestionAnswer a) =>
            new()
            {
                UserId = r.UserId,
                Name = r.User.FullName,
                OptionId = a.OptionId
            };
    }
}