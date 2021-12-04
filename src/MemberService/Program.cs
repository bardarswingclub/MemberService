using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MemberService.Data;
using MemberService.Configs;
using Microsoft.EntityFrameworkCore;

namespace MemberService
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateWebHostBuilder(args).Build();

            var config = host.Services.GetRequiredService<Config>();

            using (var scope = host.Services.CreateScope())
            {
                await scope.ServiceProvider
                    .GetRequiredService<MemberContext>()
                    .Database.MigrateAsync();

                await scope.ServiceProvider
                    .GetRequiredService<RoleManager<MemberRole>>()
                    .SeedRoles();

                await scope.ServiceProvider
                    .GetRequiredService<UserManager<User>>()
                    .SeedUserRoles(config.AdminEmails.Split(","));
            }

            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                })
                .UseStartup<Startup>();
    }
}
