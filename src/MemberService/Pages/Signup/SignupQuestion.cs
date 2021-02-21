using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Signup
{
    public partial class SignupQuestion
    {
        public Guid Id { get; set; }

        public QuestionType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<QuestionOption> Options { get; set; }

        [Expressionify]
        public static SignupQuestion Create(Data.Question q)
            => new()
            {
                Id = q.Id,
                Type = q.Type,
                Title = q.Title,
                Description = q.Description,
                Options =  q.Options.ToList()
            };
    }
}