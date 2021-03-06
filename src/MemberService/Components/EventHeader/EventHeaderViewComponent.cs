﻿using System;
using System.Linq;
using System.Threading.Tasks;
using MemberService.Data;
using MemberService.Pages.Event;
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
                    Description = e.Description,
                    SignupOpensAt = e.SignupOptions.SignupOpensAt,
                    SignupClosesAt = e.SignupOptions.SignupClosesAt,
                    IsOpen = e.IsOpen(),
                    HasClosed = e.HasClosed()
                })
                .FirstOrDefaultAsync(e => e.Id == id);

            return View(model);
        }

        public class Model
        {
            public Guid Id { get; set; }

            public String Title { get; set; }

            public string Description { get; set; }

            public bool Archived { get; set; }

            public bool HasClosed { get; set; }

            public bool IsOpen { get; set; }

            public DateTime? SignupOpensAt { get; set; }

            public DateTime? SignupClosesAt { get; set; }
        }
    }
}