using System;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event.Survey
{
    public partial class ResponseModel
    {
        public string Name { get; set; }

        public string UserId { get; set; }

        public Status Status { get; set; }

        public Guid OptionId { get; set; }

        [Expressionify]
        public static ResponseModel Create(EventSignup es, QuestionAnswer a) =>
            new()
            {
                UserId = es.Response.UserId,
                Name = es.Response.User.FullName,
                Status = es.Status,
                OptionId = a.OptionId
            };
    }
}