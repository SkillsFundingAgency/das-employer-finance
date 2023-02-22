using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.EmployerFinance.Configuration;

namespace SFA.DAS.EmployerFinance.Api.Authentication
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration config, bool isDevelopment = false)
        {
            if (isDevelopment)
            {
                services.AddAuthentication("BasicAuthentication")
                       .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
            }
            else
            {
                var azureAdConfiguration = config
                       .GetSection(ConfigurationKeys.AzureActiveDirectoryApiConfiguration)
                       .Get<AzureActiveDirectoryConfiguration>();

                var policies = new Dictionary<string, string> { { PolicyNames.Default, RoleNames.Default } };
                services.AddAuthentication(azureAdConfiguration, policies);
                services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
            }

            return services;
        }
    }
}
