using System;
using System.ComponentModel;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Semester.Survey
{
    public partial class OptionModel
    {
        public Guid Id { get; set; }

        [DisplayName("Svaralternativ")]
        public string Title { get; set; }

        [DisplayName("Beskrivelse")]
        public string Description { get; set; }

        [Expressionify]
        public static OptionModel Create(QuestionOption o)
            => new OptionModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description
            };
    }
}