using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class CommitmentsV2ApiClient : ICommitmentsV2ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CommitmentsApiV2ClientConfiguration _commitmentsApiClientConfig;
    private readonly ILogger<CommitmentsV2ApiClient> _logger;
    private readonly IConfiguration _configuration;
    private readonly AzureServiceTokenProvider _azureServiceTokenProvider;

    public CommitmentsV2ApiClient(HttpClient httpClient,
        CommitmentsApiV2ClientConfiguration commitmentsApiClientConfig,
        ILogger<CommitmentsV2ApiClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _commitmentsApiClientConfig = commitmentsApiClientConfig;
        _logger = logger;
        _configuration = configuration;
        
        _azureServiceTokenProvider = new AzureServiceTokenProvider();
    }

    public async Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId)
    {
        var url = $"{BaseUrl()}api/apprenticeships/{apprenticeshipId}";
        _logger.LogInformation("EmployerFinance Services Getting GetApprenticeship {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        _logger.LogInformation("EmployerFinance Services received response for GetApprenticeship {Url}", url);

        return JsonSerializer.Deserialize<GetApprenticeshipResponse>(json);
    }

    public async Task<GetTransferRequestSummaryResponse> GetTransferRequests(long accountId)
    {
        var url = $"{BaseUrl()}api/accounts/{accountId}/transfers";
        _logger.LogInformation("Getting GetTransferRequests {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonSerializer.Deserialize<GetTransferRequestSummaryResponse>(json);
    }

    private string BaseUrl()
    {
        if (_commitmentsApiClientConfig.ApiBaseUrl.EndsWith("/"))
        {
            return _commitmentsApiClientConfig.ApiBaseUrl;
        }

        return _commitmentsApiClientConfig.ApiBaseUrl + "/";
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        if (!_configuration["EnvironmentName"].Equals("LOCAL", StringComparison.OrdinalIgnoreCase))
        {
            var accessToken = await _azureServiceTokenProvider.GetAccessTokenAsync(_commitmentsApiClientConfig.IdentifierUri);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}