using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using MemberService.Configs;

namespace MemberService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using (var host = CreateWebHostBuilder(args).Build())
            {
                var config = host.Services.GetService<Config>();

                using (var scope = host.Services.CreateScope())
                {
                    await scope.ServiceProvider
                        .GetService<MemberContext>()
                        .Database.MigrateAsync();

                    await scope.ServiceProvider
                        .GetService<RoleManager<MemberRole>>()
                        .SeedRoles();

                    await scope.ServiceProvider
                        .GetService<UserManager<MemberUser>>()
                        .SeedUserRoles(config.Email.From);
                }

                await host.RunAsync();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
