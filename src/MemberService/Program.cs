using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MemberService.Data;
using MemberService.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Runtime.InteropServices;

namespace MemberService
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            using (var host = CreateWebHostBuilder(args).Build())
            {
                var config = host.Services.GetRequiredService<Config>();

                using (var scope = host.Services.CreateScope())
                {
                    await scope.ServiceProvider
                        .GetRequiredService<MemberContext>()
                        .Database.CreateOrMigrateAsync();

                    await scope.ServiceProvider
                        .GetRequiredService<RoleManager<MemberRole>>()
                        .SeedRoles();

                    await scope.ServiceProvider
                        .GetRequiredService<UserManager<User>>()
                        .SeedUserRoles(config.AdminEmails.Split(","));
                }

                await host.RunAsync();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                })
                .UseStartup<Startup>();

        public static Task CreateOrMigrateAsync(this DatabaseFacade database)
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? database.MigrateAsync()
                : database.EnsureCreatedAsync();
    }
}
