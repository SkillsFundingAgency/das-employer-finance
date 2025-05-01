using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Queries.GetContent;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Exceptions;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerUserAccountItem = SFA.DAS.GovUK.Auth.Employer.EmployerUserAccountItem;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetContent;

public class WhenIGetContent
{
    private Mock<IValidator<GetContentQuery>> _mockValidator;
    private Mock<ILogger<GetContentRequestHandler>> _mockLogger;
    private Mock<IContentApiClient> _mockContentApiClient;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<IAssociatedAccountsService> _mockAssociatedAccountsService;
    private EmployerFinanceWebConfiguration _configuration;
    private GetContentRequestHandler _handler;
    private GetContentQuery _query;
    private HttpContext _httpContext;
    private RouteValueDictionary _routeValues;

    private const string ContentType = "banner";
    private const string ClientId = "eas-fin";
    private const string Content = "<p>Example content</p>";

    [SetUp]
    public void Arrange()
    {
        _mockValidator = new Mock<IValidator<GetContentQuery>>();
        _mockLogger = new Mock<ILogger<GetContentRequestHandler>>();
        _mockContentApiClient = new Mock<IContentApiClient>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockAssociatedAccountsService = new Mock<IAssociatedAccountsService>();

        _configuration = new EmployerFinanceWebConfiguration
        {
            ApplicationId = ClientId,
            DefaultCacheExpirationInMinutes = 1
        };

        _routeValues = new RouteValueDictionary { { "HashedAccountId", "ABC123" } };
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.RouteValues = _routeValues;
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

        _query = new GetContentQuery { ContentType = ContentType };
        _mockValidator.Setup(x => x.Validate(_query)).Returns(new ValidationResult());

        _mockContentApiClient
            .Setup(cs => cs.Get(ContentType, ClientId))
            .ReturnsAsync(Content);

        _handler = new GetContentRequestHandler(
            _mockValidator.Object,
            _mockLogger.Object,
            _mockContentApiClient.Object,
            _configuration,
            _mockHttpContextAccessor.Object,
            _mockAssociatedAccountsService.Object);
    }

    [Test]
    public async Task WhenRequestIsValid_ThenContentApiIsCalled()
    {
        // Arrange
        SetupAccountsService();

        // Act
        await _handler.Handle(_query, CancellationToken.None);

        // Assert
        _mockContentApiClient.Verify(x => x.Get(ContentType, $"{ClientId}-levy"), Times.Once);
    }

    [Test]
    public async Task WhenRequestIsValid_ThenContentIsReturnedInResponse()
    {
        // Arrange
        SetupAccountsService();
        _mockContentApiClient.Setup(x => x.Get(ContentType, $"{ClientId}-levy")).ReturnsAsync(Content);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Content.Should().Be(Content);
        result.HasFailed.Should().BeFalse();
    }

    [Test]
    public void WhenValidationFails_ThenThrowsInvalidRequestException()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddError("ContentType", "Required");
        _mockValidator.Setup(x => x.Validate(_query)).Returns(validationResult);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(_query, CancellationToken.None));
        ex.ErrorMessages.Should().ContainKey("ContentType");
        ex.ErrorMessages["ContentType"].Should().Be("Required");
    }

    private void SetupAccountsService()
    {
        var accounts = new Dictionary<string, EmployerUserAccountItem>
        {
            { "ABC123", new EmployerUserAccountItem { ApprenticeshipEmployerType = GovUK.Auth.Employer.ApprenticeshipEmployerType.Levy } }
        };
        _mockAssociatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(accounts);
    }
}