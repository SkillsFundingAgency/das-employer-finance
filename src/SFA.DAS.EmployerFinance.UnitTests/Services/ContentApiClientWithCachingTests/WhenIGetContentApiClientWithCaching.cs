using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.ContentApiClientWithCachingTests
{
    class WhenIGetContentApiClientWithCaching
    {
        private string _contentType;
        private string _clientId;

        private Mock<IContentApiClient> MockContentApiClient;
        private Mock<ICacheService> MockCacheService;

        private string CacheKey;
        private string ContentFromCache;
        private string ContentFromApi;
        private EmployerFinanceWebConfiguration EmployerFinanceWebConfig;

        private IContentApiClient ContentApiClientWithCaching;

        [SetUp]
        public void Arrange()
        {
            _clientId = "eas-fin";
            _contentType = "banner";

            EmployerFinanceWebConfig = new EmployerFinanceWebConfiguration()
            {
                ApplicationId = "eas-fin",
                DefaultCacheExpirationInMinutes = 1
            };
            ContentFromCache = "<p> Example content from cache </p>";
            ContentFromApi = "<p> Example content from api </p>";
            CacheKey = $"{EmployerFinanceWebConfig.ApplicationId}_{_contentType}".ToLowerInvariant();

            MockContentApiClient = new Mock<IContentApiClient>();
            MockCacheService = new Mock<ICacheService>();

            ContentApiClientWithCaching = new ContentApiClientWithCaching(MockContentApiClient.Object, MockCacheService.Object, EmployerFinanceWebConfig);
        }

        [Test]
        public async Task AND_ItIsStoredInTheCache_THEN_ReturnContent()
        {
            StoredInCacheSetup();

            var result = await ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

            result.Should().Be(ContentFromCache);
            MockCacheService.Verify(c => c.TryGetAsync<string>(CacheKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AND_ItIsStoredInTheCache_THEN_ContentApiIsNotCalled()
        {
            StoredInCacheSetup();

            await ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

            MockContentApiClient.Verify(c => c.Get(_contentType, _clientId), Times.Never);
        }

        [Test]
        public async Task AND_ItIsNotStoredInTheCache_THEN_CallFromClient()
        {
            NotStoredInCacheSetup();

            var result = await ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

            MockContentApiClient.Verify(c => c.Get(_contentType, _clientId), Times.Once);
            result.Should().Be(ContentFromApi);
        }

        [Test]
        public async Task AND_ApiCallFails_THEN_ThrowException()
        {
            NotStoredInCacheSetup();
            var exception = new Exception("API Error");
            MockContentApiClient.Setup(c => c.Get(_contentType, EmployerFinanceWebConfig.ApplicationId))
                .ThrowsAsync(exception);

            var act = () => ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Failed to get content for {CacheKey}*")
                .Where(e => e.InnerException != null && e.InnerException.Message == "API Error");
        }

        [Test]
        public async Task AND_ContentIsNull_THEN_DoNotCache()
        {
            NotStoredInCacheSetup();
            MockContentApiClient.Setup(c => c.Get(_contentType, EmployerFinanceWebConfig.ApplicationId))
                .ReturnsAsync((string)null);

            await ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

            MockCacheService.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private void StoredInCacheSetup()
        {
            MockCacheService.Setup(c => c.TryGetAsync<string>(CacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, ContentFromCache));
        }

        private void NotStoredInCacheSetup()
        {
            MockCacheService.Setup(c => c.TryGetAsync<string>(CacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, (string?)null));
            MockContentApiClient.Setup(c => c.Get(_contentType, EmployerFinanceWebConfig.ApplicationId))
                .ReturnsAsync(ContentFromApi);
        }
    }
}
