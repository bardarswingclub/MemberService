using System;
using System.Linq;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Components.EventHeader
{
    public class EventHeaderViewComponent : ViewComponent
    {
        private readonly MemberContext _database;

        public EventHeaderViewComponent(MemberContext database)
        {
            _database = database;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid id)
        {
            var model = await _database.Events
                .Select(e => new Model
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description
                })
                .FirstOrDefaultAsync(e => e.Id == id);

            return View(model);
        }

        public class Model
        {
            public Guid Id { get; set; }

            public String Title { get; set; }

            public string Description { get; set; }
        }
    }
}