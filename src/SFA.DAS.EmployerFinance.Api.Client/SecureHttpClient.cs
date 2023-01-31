using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SFA.DAS.EmployerFinance.Api.Client
{
    public class SecureHttpClient : ISecureHttpClient
    {
        private readonly IEmployerFinanceApiClientConfiguration _configuration;

        public SecureHttpClient(IEmployerFinanceApiClientConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected SecureHttpClient()
        {
            // so we can mock for testing
        }

        public virtual async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            var accessToken = await GetManagedIdentityAuthenticationResult(_configuration.IdentifierUri);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static async Task<string> GetClientCredentialAuthenticationResult(string clientId, string clientSecret, string resource, string tenant)
        {
            var authority = $"https://login.microsoftonline.com/{tenant}";
            var clientCredential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, true);
            var result = await context.AcquireTokenAsync(resource, clientCredential);
            return result.AccessToken;
        }

        private static async Task<string> GetManagedIdentityAuthenticationResult(string resource)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            return await azureServiceTokenProvider.GetAccessTokenAsync(resource);
        }

        private static bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
        {
            return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenant);
        }
    }
}
