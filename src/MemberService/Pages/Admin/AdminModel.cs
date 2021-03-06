﻿using MemberService.Data;
using System.Collections.Generic;

namespace MemberService.Pages.Admin
{
    public class AdminModel
    {
        public IReadOnlyCollection<(MemberRole, IReadOnlyCollection<User>)> Roles { get; set; }
    }
}
