using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.GetContent;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetContent;

public class WhenIGetContent : QueryBaseTest<GetContentRequestHandler, GetContentRequest, GetContentResponse>
{
    public override GetContentRequest Query { get; set; }
    public override GetContentRequestHandler RequestHandler { get; set; }
    public override Mock<IValidator<GetContentRequest>> RequestValidator { get; set; }

    private string _contentType;
    private string _clientId;

    private string _cacheKey;
    private string _content;
    public EmployerFinanceConfiguration EmployerFinanceConfiguration;

    private Mock<IContentApiClient> _mockContentService;
    private Mock<ICacheStorageService> _mockCacheStorageService;

    [SetUp]
    public void Arrange()
    {
        SetUp();
        _clientId = "eas-fin";
        _contentType = "banner";

        EmployerFinanceConfiguration = new EmployerFinanceConfiguration()
        {
            ApplicationId = "eas-fin",
            DefaultCacheExpirationInMinutes = 1
        };
        _content = "<p> Example content </p>";
        _cacheKey = EmployerFinanceConfiguration.ApplicationId + "_banner";

        _mockContentService = new Mock<IContentApiClient>();
        _mockCacheStorageService = new Mock<ICacheStorageService>();
            

        _mockContentService
            .Setup(cs => cs.Get(_contentType, _clientId))
            .ReturnsAsync(_content);

        Query = new GetContentRequest
        {
            ContentType = "banner"
        };

        RequestHandler = new GetContentRequestHandler(RequestValidator.Object, Mock.Of<ILogger<GetContentRequestHandler>>(),
            _mockContentService.Object, EmployerFinanceConfiguration);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        NotStoredInCacheSetup();

        await RequestHandler.Handle(Query, CancellationToken.None);

        _mockContentService.Verify(x => x.Get(_contentType, _clientId), Times.Once);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        NotStoredInCacheSetup();

        await RequestHandler.Handle(Query, CancellationToken.None);

        _mockContentService.Verify(x => x.Get(_contentType, _clientId), Times.Once);
    }

    private void NotStoredInCacheSetup()
    {
        _mockCacheStorageService.Setup(c => c.TryGet(_cacheKey, out _content)).Returns(false);
        _mockContentService.Setup(c => c.Get("banner", _cacheKey))
            .ReturnsAsync(_content);
    }
}