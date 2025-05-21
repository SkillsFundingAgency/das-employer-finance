using System.Net.Http.Headers;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using SFA.DAS.ActiveDirectory;
using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.TokenService.Api.Client;
using IHttpClientWrapper = SFA.DAS.EmployerFinance.Interfaces.Hmrc.IHttpClientWrapper;

namespace SFA.DAS.EmployerFinance.Services;

public class HmrcService : IHmrcService
{
    private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyApiClient;
    private readonly IAzureAdAuthenticationService _azureAdAuthenticationService;
    private readonly IHmrcConfiguration _configuration;
    private readonly IHttpClientWrapper _httpClientWrapper;
    private readonly IInProcessCache _inProcessCache;
    private readonly ILogger<HmrcService> _log;
    private readonly ITokenServiceApiClient _tokenServiceApiClient;

    public HmrcService(
        IHmrcConfiguration configuration,
        IHttpClientWrapper httpClientWrapper,
        IApprenticeshipLevyApiClient apprenticeshipLevyApiClient,
        ITokenServiceApiClient tokenServiceApiClient,
        IInProcessCache inProcessCache,
        IAzureAdAuthenticationService azureAdAuthenticationService,
        ILogger<HmrcService> log)
    {
        _configuration = configuration;
        _httpClientWrapper = httpClientWrapper;
        _apprenticeshipLevyApiClient = apprenticeshipLevyApiClient;
        _tokenServiceApiClient = tokenServiceApiClient;
        _inProcessCache = inProcessCache;
        _azureAdAuthenticationService = azureAdAuthenticationService;
        _log = log;

        _httpClientWrapper.BaseUrl = _configuration.BaseUrl;
        _httpClientWrapper.AuthScheme = "Bearer";
        _httpClientWrapper.MediaTypeWithQualityHeaderValueList = new List<MediaTypeWithQualityHeaderValue> { new MediaTypeWithQualityHeaderValue("application/vnd.hmrc.1.0+json") };
    }

    public string GenerateAuthRedirectUrl(string redirectUrl)
    {
        var urlFriendlyRedirectUrl = WebUtility.UrlEncode(redirectUrl);
        return $"{_configuration.BaseUrl}oauth/authorize?response_type=code&client_id={_configuration.ClientId}&scope={_configuration.Scope}&redirect_uri={urlFriendlyRedirectUrl}";
    }

    public async Task<EmpRefLevyInformation> GetEmprefInformation(string authToken, string empRef)
    {
        try
        {
            return await _apprenticeshipLevyApiClient.GetEmployerDetails(authToken, empRef);
        }
        catch (ApiHttpException ex) when (ex.HttpCode == 404)
        {
            _log.LogInformation("Employer details not found for empRef {EmpRef}", empRef);
            return null;
        }
    }

    public async Task<EmpRefLevyInformation> GetEmprefInformation(string empRef)
    {
        var accessToken = await GetOgdAccessToken();
        return await GetEmprefInformation(accessToken, empRef);
    }

    public async Task<string> DiscoverEmpref(string authToken)
    {
        var response = await _apprenticeshipLevyApiClient.GetAllEmployers(authToken);

        if (response == null)
            return string.Empty;

        return response.Emprefs.SingleOrDefault();
    }

    public async Task<LevyDeclarations> GetLevyDeclarations(string empRef)
    {
        return await GetLevyDeclarations(empRef, null);
    }

    public async Task<LevyDeclarations> GetLevyDeclarations(string empRef, DateTime? fromDate)
    {
        try
        {
            var accessToken = await GetOgdAccessToken();

            var earliestDate = new DateTime(2017, 04, 01);
            if (!fromDate.HasValue || fromDate.Value < earliestDate) fromDate = earliestDate;

            var levyDeclarations = await _apprenticeshipLevyApiClient.GetEmployerLevyDeclarations(accessToken, empRef, fromDate);
            _log.LogDebug($"Received {levyDeclarations?.Declarations?.Count} levy declarations empRef:{empRef} fromDate:{fromDate}");
            return levyDeclarations;
        }
        catch (ApiHttpException ex) when (ex.HttpCode == 404)
        {
            _log.LogInformation("Levy declarations not found for empRef {EmpRef}", empRef);
            return new LevyDeclarations { Declarations = [] };
        }
    }

    public async Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef)
    {
        return await GetEnglishFractions(empRef, null);
    }

    public async Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef, DateTime? fromDate)
    {
        try
        {
            var accessToken = await GetOgdAccessToken();
            return await _apprenticeshipLevyApiClient.GetEmployerFractionCalculations(accessToken, empRef, fromDate);
        }
        catch (ApiHttpException ex) when (ex.HttpCode == 404)
        {
            _log.LogInformation("English fractions not found for empRef {EmpRef}", empRef);
            return new EnglishFractionDeclarations();
        }
    }

    public async Task<DateTime> GetLastEnglishFractionUpdate()
    {
        try
        {
            var hmrcLatestUpdateDate = _inProcessCache.Get<DateTime?>("HmrcFractionLastCalculatedDate");
            if (hmrcLatestUpdateDate == null)
            {
                var accessToken = await GetOgdAccessToken();
                hmrcLatestUpdateDate = await _apprenticeshipLevyApiClient.GetLastEnglishFractionUpdate(accessToken);

                if (hmrcLatestUpdateDate != null)
                    _inProcessCache.Set("HmrcFractionLastCalculatedDate", hmrcLatestUpdateDate.Value, new TimeSpan(0, 0, 30, 0));

                return hmrcLatestUpdateDate.Value;
            }
            
            return hmrcLatestUpdateDate.Value;
        }
        catch (ApiHttpException ex) when (ex.HttpCode == 404)
        {
            _log.LogInformation("Last English fraction update date not found");
            return DateTime.MinValue;
        }
    }

    private async Task<string> GetOgdAccessToken()
    {
        if (_configuration.UseHiDataFeed)
        {
            return await _azureAdAuthenticationService.GetAuthenticationResult(
                _configuration.ClientId,
                _configuration.AzureAppKey,
                _configuration.AzureResourceId,
                _configuration.AzureTenant);
        }
        else
        {
            var accessToken = await _tokenServiceApiClient.GetPrivilegedAccessTokenAsync();
            return accessToken.AccessCode;
        }
    }
}