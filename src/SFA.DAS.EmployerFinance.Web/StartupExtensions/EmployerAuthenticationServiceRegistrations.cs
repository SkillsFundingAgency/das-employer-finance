using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Handlers;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class EmployerAuthenticationServiceRegistrations
{
    public static void AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<ICustomClaims, EmployerAccountPostAuthenticationClaimsHandler>();
        services.AddTransient<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorisationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountAllRolesAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountOwnerAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, AccountActiveAuthorizationHandler>();//TODO remove after gov one login go live
        services.AddSingleton<IStubAuthenticationService, StubAuthenticationService>();//TODO remove after gov one login go live
        services.AddTransient<IUserAccountService, UserAccountService>();
            
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                PolicyNames.HasEmployerOwnerAccount
                , policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountOwnerRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                PolicyNames.HasEmployerViewerTransactorOwnerAccount
                , policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountAllRolesRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                    policy.RequireAuthenticatedUser();
                });
        });
    }
    
    public static void AddAndConfigureEmployerAuthentication(
        this IServiceCollection services,
        IdentityServerConfiguration configuration)
    {
        services
            .AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;

            }).AddOpenIdConnect(options =>
            {
                options.ClientId = configuration.ClientId;
                options.ClientSecret = configuration.ClientSecret;
                options.Authority = configuration.BaseAddress;
                options.MetadataAddress = $"{configuration.BaseAddress}/.well-known/openid-configuration";
                options.ResponseType = "code";
                options.UsePkce = false;
                        
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                        
                options.ClaimActions.MapUniqueJsonKey("sub", "id");
                options.Events.OnRemoteFailure = c =>
                {
                    if (c.Failure.Message.Contains("Correlation failed"))
                    {
                        c.Response.Redirect("/");
                        c.HandleResponse();
                    }
        
                    return Task.CompletedTask;
                };
            });
        services
            .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<ICustomClaims>((options, customClaims) =>
            {
                options.Events.OnTokenValidated = async (ctx) =>
                {
                    var claims = await customClaims.GetClaims(ctx);
                    ctx.Principal.Identities.First().AddClaims(claims);
                };
            });
                
        services.AddAuthentication().AddCookie(options =>
        {
            options.AccessDeniedPath = new PathString("/error/403");
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.Cookie.Name = $"SFA.DAS.EmployerFinance.Web.Auth";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.Cookie.SameSite = SameSiteMode.None;
            options.CookieManager = new ChunkingCookieManager { ChunkSize = 3000 };
        });
    } 
}