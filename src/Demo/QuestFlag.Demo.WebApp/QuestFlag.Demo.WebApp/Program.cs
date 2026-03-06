using QuestFlag.Demo.WebApp.Client.Pages;
using QuestFlag.Demo.WebApp.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using QuestFlag.Demo.WebApp.State;
using QuestFlag.Infrastructure.Client;
using QuestFlag.Infrastructure.Client.Contracts;
using QuestFlag.Passport.UserClient;
using QuestFlag.Infrastructure.ApiCore.StartupExtensions;
using QuestFlag.Communication.Client.Implementations;
using QuestFlag.Communication.Client.Contracts;

namespace QuestFlag.Demo.WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddQuestFlagApiServices();
        builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddHttpContextAccessor();

        var passportServicesUrl = builder.Configuration["ServiceUrls:PassportServices"]
            ?? throw new InvalidOperationException("ServiceUrls:PassportServices is required in configuration.");

        var infraServicesUrl = builder.Configuration["ServiceUrls:InfraServices"]
            ?? throw new InvalidOperationException("ServiceUrls:InfraServices is required in configuration.");

        builder.Services.AddScoped<IAccessTokenProvider, ServerTokenProvider>();
        builder.Services.AddTransient<QuestFlag.Infrastructure.Client.AuthenticatedHttpHandler>();

        builder.Services.AddHttpClient<IUploadApiService, UploadApiService>(client =>
        {
            client.BaseAddress = new Uri(infraServicesUrl);
        }).AddHttpMessageHandler<QuestFlag.Infrastructure.Client.AuthenticatedHttpHandler>();

        builder.Services.AddHttpClient<PassportUserClient>(client =>
        {
            client.BaseAddress = new Uri(passportServicesUrl);
        }).AddHttpMessageHandler<QuestFlag.Infrastructure.Client.AuthenticatedHttpHandler>();

        var oidcSettings = builder.Configuration.GetSection(QuestFlag.Passport.Domain.Models.OidcSettings.SectionName).Get<QuestFlag.Passport.Domain.Models.OidcSettings>()
            ?? throw new InvalidOperationException($"'{QuestFlag.Passport.Domain.Models.OidcSettings.SectionName}' configuration section is required.");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = oidcSettings.Authority;
            options.ClientId = oidcSettings.ClientId;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.Scope.Add("roles");
            options.Scope.Add("offline_access");
            
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = "role";

            // After successful server-side OIDC, redirect to the home page.
            options.Events.OnTicketReceived = ctx =>
            {
                ctx.ReturnUri = "/";
                return Task.CompletedTask;
            };

            // Handle failed remote logins gracefully (e.g., user rejected, state mismatch).
            options.Events.OnRemoteFailure = ctx =>
            {
                var msg = Uri.EscapeDataString(ctx.Failure?.Message ?? "Authentication failed");
                ctx.Response.Redirect($"/login?ErrorMessage={msg}");
                ctx.HandleResponse();
                return Task.CompletedTask;
            };
        });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseQuestFlagApiPipeline();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(QuestFlag.Demo.WebApp.Client._Imports).Assembly);


        // Server-side login — triggers ASP.NET Core OIDC challenge.
        // The middleware handles PKCE via correlation cookies (no localStorage needed).
        app.MapGet("/api/auth/login", async (HttpContext context) =>
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/" });
        }).AllowAnonymous();

        // Server-side logout — clears Demo App cookie then clears Passport Services session.
        app.MapGet("/api/auth/logout", async (HttpContext context) =>
        {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, properties);
        }).AllowAnonymous();

        // Called by OpenIddict after it clears the Passport Services session.
        // Redirects the user to the demo app landing page.
        app.MapGet("/signout-callback-oidc", () => Results.Redirect("/")).AllowAnonymous();

        app.Run();
    }
}
