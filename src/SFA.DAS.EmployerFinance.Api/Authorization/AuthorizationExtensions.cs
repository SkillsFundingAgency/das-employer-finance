using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmployerFinance.Api.Authorization;

public static class AuthorizationExtensions
{
    private const string DefaultPolicyName = "default";

    private static readonly string[] PolicyNames =
    {
        ApiRoles.ReadAllEmployerAccountBalances,
        ApiRoles.ReadUserAccounts
    };

    public static IServiceCollection AddApiAuthorization(this IServiceCollection services, bool isDevelopment = false)
    {
        services.AddAuthorization(x =>
        {
            AddDefaultPolicy(isDevelopment, x);

            AddApiPolicies(isDevelopment, x);

            x.DefaultPolicy = x.GetPolicy(DefaultPolicyName);

        });

        if (isDevelopment)
        {
            services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
        }

        return services;
    }

    private static void AddApiPolicies(bool isDevelopment, AuthorizationOptions x)
    {
        foreach (var policyName in PolicyNames)
        {
            x.AddPolicy(policyName, policy =>
            {
                if (isDevelopment)
                {
                    policy.AllowAnonymousUser();
                }
                else
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole(policyName);
                }
            });
        }
    }

    private static void AddDefaultPolicy(bool isDevelopment, AuthorizationOptions x)
    {
        x.AddPolicy(DefaultPolicyName, policy =>
        {
            if (isDevelopment)
            {
                policy.AllowAnonymousUser();
            }
            else
            {
                policy.RequireAuthenticatedUser();
            }
        });
    }
}