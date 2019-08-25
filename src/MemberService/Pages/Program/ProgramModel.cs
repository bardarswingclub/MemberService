using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Program
{
    public class ProgramModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ProgramType Type { get; set; }

        public IReadOnlyList<EventModel> Events { get; set; } = new List<EventModel>();

        [Expressionify]
        public static ProgramModel Create(Data.Program program) => new ProgramModel
        {
            Id = program.Id,
            Title = program.Title,
            Description = program.Description,
            Type = program.Type,
            Events = program.Events.Select(e => new EventModel
            {
                Id = e.Id,
                Title = e.Title
            }).ToList()
        };
    }
}
