﻿using System.Net.Http.Headers;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using Newtonsoft.Json;
using SFA.DAS.ActiveDirectory;
using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Policies.Hmrc;
using SFA.DAS.TokenService.Api.Client;
using static Microsoft.Azure.Amqp.CbsConstants;
using ExecutionPolicy = SFA.DAS.EmployerFinance.Policies.Hmrc.ExecutionPolicy;
using IHttpClientWrapper = SFA.DAS.EmployerFinance.Interfaces.Hmrc.IHttpClientWrapper;

namespace SFA.DAS.EmployerFinance.Services;

public class HmrcService : IHmrcService
{
    private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyApiClient;
    private readonly IAzureAdAuthenticationService _azureAdAuthenticationService;
    private readonly IHmrcConfiguration _configuration;
    private readonly ExecutionPolicy _executionPolicy;
    private readonly IHttpClientWrapper _httpClientWrapper;
    private readonly IInProcessCache _inProcessCache;
    private readonly ILogger<HmrcService> _log;
    private readonly ITokenServiceApiClient _tokenServiceApiClient;


    public HmrcService(
        IHmrcConfiguration configuration,
        IHttpClientWrapper httpClientWrapper,
        IApprenticeshipLevyApiClient apprenticeshipLevyApiClient,
        ITokenServiceApiClient tokenServiceApiClient,
        [RequiredPolicy(HmrcExecutionPolicy.Name)]
        ExecutionPolicy executionPolicy,
        IInProcessCache inProcessCache,
        IAzureAdAuthenticationService azureAdAuthenticationService,
        ILogger<HmrcService> log)
    {
        _configuration = configuration;
        _httpClientWrapper = httpClientWrapper;
        _apprenticeshipLevyApiClient = apprenticeshipLevyApiClient;
        _tokenServiceApiClient = tokenServiceApiClient;
        _executionPolicy = executionPolicy;
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

    public Task<EmpRefLevyInformation> GetEmprefInformation(string authToken, string empRef)
    {
        return _executionPolicy.ExecuteAsync(async () => await _apprenticeshipLevyApiClient.GetEmployerDetails(authToken, empRef));
    }

    public async Task<EmpRefLevyInformation> GetEmprefInformation(string empRef)
    {
        var accessToken = await _executionPolicy.ExecuteAsync(async () => await GetOgdAccessToken());

        return await GetEmprefInformation(accessToken, empRef);
    }

    public Task<string> DiscoverEmpref(string authToken)
    {
        return _executionPolicy.ExecuteAsync(async () =>
        {
            var response = await _apprenticeshipLevyApiClient.GetAllEmployers(authToken);

            if (response == null)
                return string.Empty;

            return response.Emprefs.SingleOrDefault();
        });
    }

    public Task<LevyDeclarations> GetLevyDeclarations(string empRef)
    {
        return GetLevyDeclarations(empRef, null);
    }

    public Task<LevyDeclarations> GetLevyDeclarations(string empRef, DateTime? fromDate)
    {
        return _executionPolicy.ExecuteAsync(async () =>
        {
            var accessToken = await GetOgdAccessToken();

            var earliestDate = new DateTime(2017, 04, 01);
            if (!fromDate.HasValue || fromDate.Value < earliestDate) fromDate = earliestDate;

            var levyDeclartions = await _executionPolicy.ExecuteAsync(async () =>
            {
                var response = await _apprenticeshipLevyApiClient.GetEmployerLevyDeclarations(accessToken, empRef, fromDate);
                return response;
            });

            _log.LogDebug($"Received {levyDeclartions?.Declarations?.Count} levy declarations empRef:{empRef} fromDate:{fromDate}");
            return levyDeclartions;
        });
    }

    public Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef)
    {
        return GetEnglishFractions(empRef, null);
    }

    public Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef, DateTime? fromDate)
    {
        return _executionPolicy.ExecuteAsync(async () =>
        {
            var accessToken = await GetOgdAccessToken();

            return await _apprenticeshipLevyApiClient.GetEmployerFractionCalculations(accessToken, empRef, fromDate);
        });
    }

    public async Task<DateTime> GetLastEnglishFractionUpdate()
    {
        var hmrcLatestUpdateDate = _inProcessCache.Get<DateTime?>("HmrcFractionLastCalculatedDate");
        if (hmrcLatestUpdateDate == null)
            return await _executionPolicy.ExecuteAsync(async () =>
            {
                var accessToken = await GetOgdAccessToken();

                hmrcLatestUpdateDate = await _apprenticeshipLevyApiClient.GetLastEnglishFractionUpdate(accessToken);

                if (hmrcLatestUpdateDate != null) _inProcessCache.Set("HmrcFractionLastCalculatedDate", hmrcLatestUpdateDate.Value, new TimeSpan(0, 0, 30, 0));

                return hmrcLatestUpdateDate.Value;
            });
        return hmrcLatestUpdateDate.Value;
    }

    public async Task<string> GetOgdAccessToken()
    {
        if (_configuration.UseHiDataFeed)
        {
            var accessToken =
                await _azureAdAuthenticationService.GetAuthenticationResult(_configuration.ClientId,
                    _configuration.AzureAppKey, _configuration.AzureResourceId,
                    _configuration.AzureTenant);

            return accessToken;
        }
        else
        {
            var accessToken = await _tokenServiceApiClient.GetPrivilegedAccessTokenAsync();
            return accessToken.AccessCode;
        }
    }
}