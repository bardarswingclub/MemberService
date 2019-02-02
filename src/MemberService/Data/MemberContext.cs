using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data
{
    public class MemberContext : IdentityDbContext
    {
        public MemberContext(DbContextOptions<MemberContext> options)
            : base(options)
        {
        }
    }
}
