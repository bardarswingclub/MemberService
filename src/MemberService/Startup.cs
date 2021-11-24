using System;

using Clave.NamespaceViewLocationExpander;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MemberService.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MemberService.Configs;
using MemberService.Auth;
using System.Globalization;
using System.Reflection;

using Microsoft.AspNetCore.Localization;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using MemberService.Auth.Requirements;

namespace MemberService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }
        private bool IsDevelopment => HostingEnvironment.IsDevelopment();

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped(typeof(IEmailer), IsDevelopment ? typeof(DummyConsoleEmailer) : typeof(SendGridEmailer))
                .AddScoped<IEmailSender, EmailSender>()
                .AddScoped(e => new SendGridClient(Configuration["Email:SendGridApiKey"]))
                .AddScoped<Stripe.ChargeService>()
                .AddScoped<Stripe.Checkout.SessionService>()
                .AddScoped<Stripe.CustomerService>()
                .AddScoped<Stripe.PaymentIntentService>()
                .AddScoped<Stripe.RefundService>()
                .AddScoped<IPaymentService, PaymentService>()
                .AddScoped<ILoginService, LoginService>()
                .AddScoped<IPartialRenderer, PartialRenderer>()
                .AddScoped<IEmailService, EmailService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .AddScoped(x => x
                    .GetRequiredService<IUrlHelperFactory>()
                    .GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext));

            services.AddSingleton(Configuration.Get<Config>());

            services.AddDbContext<MemberContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services
                .AddIdentity<User, MemberRole>(config =>
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
                .AddPasswordlessLoginTokenProvider(IsDevelopment);

            services
                .AddRazorPages()
                .AddRazorOptions(options =>
            {
                options.ViewLocationFormats.Add("/{0}.cshtml");
                options.PageViewLocationFormats.Add("/{0}.cshtml");
            });

            if (HostingEnvironment.IsDevelopment())
            {
                services
                    .AddControllersWithViews()
                    .AddRazorRuntimeCompilation();
            }
            else
            {
                services
                    .AddControllersWithViews();
            }

            services
                .AddScoped<IAuthorizationHandler, RequirementsHandler>()
                .AddAuthorization(options =>
                {
                    foreach (var policy in Enum.GetValues<Policy>())
                    {
                        options.AddPolicy(policy.ToString(), b => b.AddRequirements(new Requirement(policy)));
                    }
                });

            services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                })
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["Authentication:Facebook:AppId"];
                    options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                });

            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();

            services.UseNamespaceViewLocations();
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/account/login";
                options.LogoutPath = $"/account/logout";
                options.AccessDeniedPath = $"/account/accessDenied";
            });

            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Config config)
        {
            Stripe.StripeConfiguration.ApiKey = config.Stripe.SecretKey;

            if (IsDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            var supportedCultures = new[]
            {
                new CultureInfo("nb-NO")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("nb-NO"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
