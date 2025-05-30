﻿using System.Globalization;
using System.Security.Claims;

using Clave.Expressionify;
using Clave.NamespaceViewLocationExpander;

using MemberService;
using MemberService.Auth;
using MemberService.Auth.Requirements;
using MemberService.Configs;
using MemberService.Data;
using MemberService.Services;
using MemberService.Services.Vipps;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

using SendGrid;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var env = builder.Environment;
var services = builder.Services;
var configuration = builder.Configuration;
var config = configuration.Get<Config>();

Stripe.StripeConfiguration.ApiKey = config.Stripe.SecretKey;

services
    .AddSingleton(config)
    .AddScoped(typeof(IEmailer), env.IsDevelopment() ? typeof(DummyConsoleEmailer) : typeof(SendGridEmailer))
    .AddScoped<IEmailSender, EmailSender>()
    .AddScoped<SendGridClient>(e => new SendGridClient(config.Email.SendGridApiKey))
    .AddScoped<Stripe.ChargeService>()
    .AddScoped<Stripe.Checkout.SessionService>()
    .AddScoped<Stripe.CustomerService>()
    .AddScoped<Stripe.PaymentIntentService>()
    .AddScoped<Stripe.RefundService>()
    .AddScoped<IStripePaymentService, StripePaymentService>()
    .AddScoped<ILoginService, LoginService>()
    .AddScoped<IPartialRenderer, PartialRenderer>()
    .AddScoped<IEmailService, EmailService>()
    .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
    .AddScoped<IVippsClient, VippsClient>()
    .AddScoped<IVippsPaymentService, VippsPaymentService>()
    .AddSingleton<AccessTokenCache>()
    .AddTransient<AccessTokenHandler>()
    .AddScoped(x => x
        .GetRequiredService<IUrlHelperFactory>()
        .GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext));

services.AddHttpClient("Vipps-auth", client =>
{
    client.BaseAddress = new Uri(config.Vipps.BaseUrl);
    client.DefaultRequestHeaders.Add("client_id", config.Authentication.Vipps.ClientId);
    client.DefaultRequestHeaders.Add("client_secret", config.Authentication.Vipps.ClientSecret);
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.Vipps.SubscriptionKey);
    client.Timeout = TimeSpan.FromSeconds(5);
});

services.AddHttpClient("Vipps", client =>
{
    client.BaseAddress = new Uri(config.Vipps.BaseUrl);
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.Vipps.SubscriptionKey);
    client.DefaultRequestHeaders.Add("Merchant-Serial-Number", config.Vipps.MerchantSerialNumber);
}).AddHttpMessageHandler<AccessTokenHandler>();

services
    .AddDbContext<MemberContext>(o => o
        .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
        .UseExpressionify(o => o.WithEvaluationMode(ExpressionEvaluationMode.FullCompatibilityButSlow)));

services
    .AddDataProtection()
    .PersistKeysToDbContext<MemberContext>();

services
    .AddIdentity<User, MemberRole>(config =>
    {
        config.SignIn.RequireConfirmedEmail = true;
        config.User.RequireUniqueEmail = true;

        config.Password = new()
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
    .AddPasswordlessLoginTokenProvider(env.IsDevelopment())
    .AddClaimsPrincipalFactory<UserClaimsPrincipalFactory>();

services
    .AddRazorPages()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/{0}.cshtml");
        options.PageViewLocationFormats.Add("/{0}.cshtml");
    })
    .AddDataAnnotationsLocalization();

services
    .UseNamespaceViewLocations()
    .AddControllersWithViews()
    .IfDev(env, s => s.AddRazorRuntimeCompilation())
    .AddViewLocalization(o =>
    {
        o.ResourcesPath = "";
    })
    .AddDataAnnotationsLocalization(o =>
    {
        o.DataAnnotationLocalizerProvider = (type, localizer) =>
        {
            while (type.DeclaringType != null) type = type.DeclaringType;
            return localizer.Create(type);
        };
    });

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

services
    .AddAuthentication()
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = config.Authentication.Microsoft.ClientId;
        options.ClientSecret = config.Authentication.Microsoft.ClientSecret;
    })
    .AddFacebook(options =>
    {
        options.AppId = config.Authentication.Facebook.AppId;
        options.AppSecret = config.Authentication.Facebook.AppSecret;
        options.AccessDeniedPath = "/account/accessDenied";
    })
    /*.AddOpenIdConnect("Vipps", "Vipps", options =>
    {
        options.Authority = $"{config.Vipps.BaseUrl}/access-management-1.0/access/";
        options.ClientId = config.Authentication.Vipps.ClientId;
        options.ClientSecret = config.Authentication.Vipps.ClientSecret;
        options.AccessDeniedPath = "/account/accessDenied";
        options.CallbackPath = "/signin-vipps";
        options.ResponseType = "code";
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("name");
        //options.Scope.Add("api_version_2");
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
    })*/;

services
    .Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    })
    .ConfigureApplicationCookie(options =>
    {
        options.LoginPath = $"/account/login";
        options.LogoutPath = $"/account/logout";
        options.AccessDeniedPath = $"/account/accessDenied";
    });

services
    .AddApplicationInsightsTelemetry()
    .AddApplicationInsightsTelemetryProcessor<Fix404Filter>()
    .ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
    {
        module.EnableSqlCommandTextInstrumentation = true;
    });

services
    .AddLocalization()
    .Configure<RequestLocalizationOptions>(o => {
        var cultures = new CultureInfo[]
        {
            new("nb"),
            new("en")
        };
        o.DefaultRequestCulture = new("nb");
        // Formatting numbers, dates, etc.
        o.SupportedCultures = cultures;
        // UI strings that we have localized.
        o.SupportedUICultures = cultures;
    });

services.
    Configure<RecaptchaSettings>(builder.Configuration.GetSection("GoogleRecaptcha"));

var app = builder.Build();

Global.Services = app.Services;

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

app
    .IfProd(env, a => a
        .UseExceptionHandler("/Home/Error")
        .UseHsts())
    .UseHttpsRedirection()
    .UseStaticFiles()
    .UseRouting()
    .UseRequestLocalization()
    .UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}")
    .UseAuthentication()
    .UseAuthorization()
    .Use(EnsureUserHasFullName)
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        endpoints.MapRazorPages();
    });

await app.RunAsync();

static Task EnsureUserHasFullName(HttpContext ctx, Func<Task> next)
{
    if (!ctx.User.Identity.IsAuthenticated)
        return next();

    if (!string.IsNullOrWhiteSpace(ctx.User?.FindFirstValue("FullName")))
        return next();

    if (ctx.GetEndpoint()?.DisplayName == "/Account/Register")
        return next();

    ctx.Response.Redirect($"/Account/Register?returnUrl={ctx.Request.Path}");
    return Task.CompletedTask;
}

public static class Global
{
    public static IServiceProvider Services { get; set; }
}

public class Fix404Filter : ITelemetryProcessor
{
    private ITelemetryProcessor Next { get; set; }

    // next will point to the next TelemetryProcessor in the chain.
    public Fix404Filter(ITelemetryProcessor next)
    {
        Next = next;
    }

    public void Process(ITelemetry item)
    {
        if(item is RequestTelemetry requestItem)
        {
            var method = requestItem.Name.Split(' ').FirstOrDefault("UNKNOWN");

            requestItem.Name = $"{method} {requestItem.Url.AbsolutePath}";
        }

        Next.Process(item);
    }
}

public class RecaptchaSettings
{
    public string SiteKey { get; set; }
    public string SecretKey { get; set; }
}