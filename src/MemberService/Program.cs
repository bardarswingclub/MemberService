using System.Globalization;

using Clave.NamespaceViewLocationExpander;

using MemberService.Auth;
using MemberService.Auth.Requirements;
using MemberService.Configs;
using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

using SendGrid;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var services = builder.Services;

services
    .AddScoped(typeof(IEmailer), builder.Environment.IsDevelopment() ? typeof(DummyConsoleEmailer) : typeof(SendGridEmailer))
    .AddScoped<IEmailSender, EmailSender>()
    .AddScoped<SendGridClient>(e => new SendGridClient(builder.Configuration["Email:SendGridApiKey"]))
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

services.AddSingleton<global::MemberService.Configs.Config>((global::MemberService.Configs.Config)builder.Configuration.Get<global::MemberService.Configs.Config>());

services.AddDbContext<MemberContext>(o => SqlServerDbContextOptionsExtensions.UseSqlServer(o, (string)builder.Configuration.GetConnectionString((string)"DefaultConnection")));

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
    .AddPasswordlessLoginTokenProvider(builder.Environment.IsDevelopment());

services
    .AddRazorPages()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/{0}.cshtml");
        options.PageViewLocationFormats.Add("/{0}.cshtml");
    });

if (builder.Environment.IsDevelopment())
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
    .AddScoped<IAuthorizationHandler, RoleRequirementsHandler>()
    .AddScoped<IAuthorizationHandler, EventRequirementsHandler>()
    .AddScoped<IAuthorizationHandler, SemesterRequirementsHandler>()
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
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        options.AccessDeniedPath = "/account/accessDenied";
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

var app = builder.Build();

var config = app.Services.GetRequiredService<Config>();

using (var scope = app.Services.CreateScope())
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

Stripe.StripeConfiguration.ApiKey = config.Stripe.SecretKey;

if (!builder.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

var supportedCultures = new[] { new CultureInfo("nb-NO") };

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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

await app.RunAsync();
