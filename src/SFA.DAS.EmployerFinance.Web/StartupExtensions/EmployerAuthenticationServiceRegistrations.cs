using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.GovUK.Auth.Services;
using EmployerClaims = SFA.DAS.EmployerFinance.Infrastructure.EmployerClaims;

namespace SFA.DAS.EmployerFinance.Web.StartupExtensions;

public static class EmployerAuthenticationServiceRegistrations
{
    public static void AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorisationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountAllRolesAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountOwnerAuthorizationHandler>();
        services.AddSingleton<IStubAuthenticationService, StubAuthenticationService>();//TODO remove after gov one login go live
        services.AddTransient<IGovAuthEmployerAccountService, UserAccountService>();
            
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                PolicyNames.HasEmployerOwnerAccount
                , policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountOwnerRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                });
            options.AddPolicy(
                PolicyNames.HasEmployerViewerTransactorOwnerAccount
                , policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountAllRolesRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                });
        });
    }
}