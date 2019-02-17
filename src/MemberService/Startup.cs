using Clave.NamespaceViewLocationExpander;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberService.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using Stripe;
using MemberService.Configs;
using Microsoft.AspNetCore.Identity.UI.Services;
using MemberService.Auth;

namespace MemberService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ChargeService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton(Configuration.Get<Config>());
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddDbContext<MemberContext>(ConfigureConnectionString);

            services.AddIdentity<MemberUser, MemberRole>(config =>
                {
                    config.SignIn.RequireConfirmedEmail = true;
                    config.User.RequireUniqueEmail = true;

                    config.Password = new PasswordOptions
                    {
                        RequireDigit = false,
                        RequireLowercase = false,
                        RequireNonAlphanumeric = false,
                        RequireUppercase = false,
                        RequiredLength = 4
                    };
                })
                .AddRoles<MemberRole>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<MemberContext>()
                .AddPasswordlessLoginTokenProvider();

            services.UseNamespaceViewLocations();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/identity/account/login";
                options.LogoutPath = $"/identity/account/logout";
                options.AccessDeniedPath = $"/identity/account/accessDenied";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Config config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            StripeConfiguration.SetApiKey(config.Stripe.SecretKey);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void ConfigureConnectionString(DbContextOptionsBuilder options)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.UseSqlite("Data Source=members.db");
            }
            else
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            }
        }
    }
}
