﻿using System.Net;
using HMRC.ESFA.Levy.Api.Client;
using Newtonsoft.Json;
using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.Hmrc;

public class WhenICallTheHmrcServiceForAuthentication
{
    private const string ExpectedAccessCode = "789654321AGFVD";
    private const string ExpectedBaseUrl = "http://hmrcbase.gov.uk/";
    private const string ExpectedClientId = "654321";
    private const string ExpectedClientSecret = "my_secret";
    private const string ExpectedOgdClientId = "123456789";
    private const string ExpectedScope = "emp_ref";
    private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyApiClient;
    private HmrcConfiguration _configuration;
    private HmrcService _hmrcService;
    private Mock<IHttpClientWrapper> _httpClientWrapper;
    private Mock<ITokenServiceApiClient> _tokenService;

    [SetUp]
    public void Arrange()
    {
        _configuration = new HmrcConfiguration
        {
            BaseUrl = ExpectedBaseUrl,
            ClientId = ExpectedClientId,
            OgdClientId = ExpectedOgdClientId,
            Scope = ExpectedScope,
            ClientSecret = ExpectedClientSecret
        };

        _httpClientWrapper = new Mock<IHttpClientWrapper>();
        _httpClientWrapper.Setup(x => x.SendMessage(It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(JsonConvert.SerializeObject(new HmrcTokenResponse()));

        _tokenService = new Mock<ITokenServiceApiClient>();
        _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(new PrivilegedAccessToken { AccessCode = ExpectedAccessCode });

        _apprenticeshipLevyApiClient = new Mock<IApprenticeshipLevyApiClient>();

        var inProcessCache = new Mock<IInProcessCache>();
        _hmrcService = new HmrcService(_configuration, _httpClientWrapper.Object,
            _apprenticeshipLevyApiClient.Object, _tokenService.Object, inProcessCache.Object, null, new Mock<ILogger<HmrcService>>().Object);
    }

    [Test]
    public void ThenTheAuthUrlIsGeneratedFromTheStoredConfigValues()
    {
        //Arrange
        var redirectUrl = "http://mytestUrl.to.redirectto?a=564kjg";
        var urlFriendlyRedirectUrl = WebUtility.UrlEncode(redirectUrl);

        //Assert
        var actual = _hmrcService.GenerateAuthRedirectUrl(redirectUrl);

        //Assert
        actual.Should().Be($"{ExpectedBaseUrl}oauth/authorize?response_type=code&client_id={ExpectedClientId}&scope={ExpectedScope}&redirect_uri={urlFriendlyRedirectUrl}");
    }
}