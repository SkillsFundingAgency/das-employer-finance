using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Authentication.Extensions.Legacy;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class ContentApiClient : ApiClientBase, IContentApiClient
{
    private readonly string _apiBaseUrl;
    private readonly string _identifierUri;
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;

    public ContentApiClient(HttpClient client, IContentClientApiConfiguration contentClientConfiguration, IConfiguration configuration) : base(client) 
    {
        _apiBaseUrl = contentClientConfiguration.ApiBaseUrl.EndsWith("/")
            ? contentClientConfiguration.ApiBaseUrl
            : contentClientConfiguration.ApiBaseUrl + "/";

        _identifierUri = contentClientConfiguration.IdentifierUri;
        _client = client;
        _configuration = configuration;
    }

    private async Task AddAuthenticationHeader()
    {
        if (_configuration["EnvironmentName"] != "LOCAL")
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(_identifierUri);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    public async Task<string> Get(string type, string applicationId)
    {
        await AddAuthenticationHeader();

        var uri = $"{_apiBaseUrl}api/content?applicationId={applicationId}&type={type}";
            
        return await GetAsync(uri); 
    }
}