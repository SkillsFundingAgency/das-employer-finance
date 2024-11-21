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
        private Mock<ICacheStorageService> MockCacheStorageService;

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
            CacheKey = EmployerFinanceWebConfig.ApplicationId + "_banner";

            MockContentApiClient = new Mock<IContentApiClient>();
            MockCacheStorageService = new Mock<ICacheStorageService>();

            ContentApiClientWithCaching = new ContentApiClientWithCaching(MockContentApiClient.Object, MockCacheStorageService.Object, EmployerFinanceWebConfig);
        }
        [Test]
        public async Task AND_ItIsStoredInTheCache_THEN_ReturnContent()
        {
            StoredInCacheSetup();

            var result = await ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

            result.Should().Be(ContentFromCache);
            MockCacheStorageService.Verify(c => c.TryGet(CacheKey, out ContentFromCache), Times.Once);
        }

        [Test]
        public async Task AND_ItIsStoredInTheCache_THEN_ContentApiIsNotCalled()
        {
            StoredInCacheSetup();

            var result = await ContentApiClientWithCaching.Get(_contentType, EmployerFinanceWebConfig.ApplicationId);

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
        private void StoredInCacheSetup()
        {
            MockCacheStorageService.Setup(c => c.TryGet(CacheKey, out ContentFromCache)).Returns(true);
            MockContentApiClient.Setup(c => c.Get("banner", CacheKey));
        }

        private void NotStoredInCacheSetup()
        {

            MockCacheStorageService.Setup(c => c.TryGet(CacheKey, out ContentFromCache)).Returns(false);
            MockContentApiClient.Setup(c => c.Get("banner", EmployerFinanceWebConfig.ApplicationId))
                .ReturnsAsync(ContentFromApi);
        }
    }
}
