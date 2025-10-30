using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.EmployerFinance.Api.Client;

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

        using var client = new HttpClient();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }


    private static async Task<string> GetManagedIdentityAuthenticationResult(string resource)
    {
        var azureServiceTokenProvider = new ChainedTokenCredential();
        var accessToken = await azureServiceTokenProvider
            .GetTokenAsync(new TokenRequestContext(scopes: new string[] {resource}));
        return accessToken.Token;
    }

}