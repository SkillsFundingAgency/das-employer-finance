using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Exceptions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.GetContent;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetContent;

public class GetContentRequestHandlerTests
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
    private const string HashedAccountId = "ABC123";
    private const string ContentType = "banner";
    private const string ContentResponse = "<p>Test content</p>";

    [SetUp]
    public void Setup()
    {
        _mockValidator = new Mock<IValidator<GetContentQuery>>();
        _mockLogger = new Mock<ILogger<GetContentRequestHandler>>();
        _mockContentApiClient = new Mock<IContentApiClient>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockAssociatedAccountsService = new Mock<IAssociatedAccountsService>();
        _configuration = new EmployerFinanceWebConfiguration { ApplicationId = "das-employerfinance-web" };

        _routeValues = new RouteValueDictionary { { "HashedAccountId", HashedAccountId } };
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.RouteValues = _routeValues;
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);

        _query = new GetContentQuery { ContentType = ContentType };
        _mockValidator
            .Setup(x => x.ValidateAsync(_query))
            .ReturnsAsync(new ValidationResult());

        _handler = new GetContentRequestHandler(
            _mockValidator.Object,
            _mockLogger.Object,
            _mockContentApiClient.Object,
            _configuration,
            _mockHttpContextAccessor.Object,
            _mockAssociatedAccountsService.Object);
    }

    [Test]
    public void Handle_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddError("ContentType", "Required");
        _mockValidator
            .Setup(x => x.ValidateAsync(_query))
            .ReturnsAsync(validationResult);

        // Act & Assert
        Func<Task> act = async () => await _handler.Handle(_query, CancellationToken.None);
        act.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("Invalid request")
            .Result.Which.ErrorMessages.Should().ContainKey("ContentType")
            .And.Subject["ContentType"].Should().Be("Required");
    }

    [Test]
    public async Task Handle_WhenNoHashedAccountId_ReturnsEmptyResponse()
    {
        // Arrange
        _routeValues.Clear();

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Content.Should().BeNull();
        result.HasFailed.Should().BeFalse();
    }

    [Test]
    public async Task Handle_WhenAccountIsLevy_UsesLevyApplicationId()
    {
        // Arrange
        var accounts = new Dictionary<string, EmployerUserAccountItem>
        {
            { HashedAccountId, new EmployerUserAccountItem { ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy } }
        };
        _mockAssociatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(accounts);
        var expectedApplicationId = "das-employerfinance-web-levy";
        _mockContentApiClient.Setup(x => x.Get(ContentType, expectedApplicationId)).ReturnsAsync(ContentResponse);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Content.Should().Be(ContentResponse);
        result.HasFailed.Should().BeFalse();
        _mockContentApiClient.Verify(x => x.Get(ContentType, expectedApplicationId), Times.Once);
    }

    [Test]
    public async Task Handle_WhenAccountIsNonLevy_UsesNonLevyApplicationId()
    {
        // Arrange
        var accounts = new Dictionary<string, EmployerUserAccountItem>
        {
            { HashedAccountId, new EmployerUserAccountItem { ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy } }
        };
        _mockAssociatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(accounts);
        var expectedApplicationId = "das-employerfinance-web-nonlevy";
        _mockContentApiClient.Setup(x => x.Get(ContentType, expectedApplicationId)).ReturnsAsync(ContentResponse);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Content.Should().Be(ContentResponse);
        result.HasFailed.Should().BeFalse();
        _mockContentApiClient.Verify(x => x.Get(ContentType, expectedApplicationId), Times.Once);
    }

    [Test]
    public async Task Handle_WhenContentApiThrowsException_ReturnsFailedResponse()
    {
        // Arrange
        var accounts = new Dictionary<string, EmployerUserAccountItem>
        {
            { HashedAccountId, new EmployerUserAccountItem { ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy } }
        };
        _mockAssociatedAccountsService.Setup(x => x.GetAccounts(false)).ReturnsAsync(accounts);
        _mockContentApiClient.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test error"));

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.HasFailed.Should().BeTrue();
        result.Content.Should().BeNull();
    }
} 