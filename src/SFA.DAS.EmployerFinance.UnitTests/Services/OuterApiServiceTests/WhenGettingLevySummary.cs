using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services;
using System.Net.Http;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.OuterApiServiceTests;

[TestFixture]
internal class WhenGettingLevySummary
{
    private Mock<IOuterApiClient> _mockApiClient;
    private Mock<IInProcessCache> _mockCache;
    private OuterApiService _outerApiService;

    private const string HashedAccountId = "ABC123";
    private static string CacheKey => $"LevySummary_{HashedAccountId}";

    [SetUp]
    public void Arrange()
    {
        _mockApiClient = new Mock<IOuterApiClient>();
        _mockCache = new Mock<IInProcessCache>();

        _outerApiService = new OuterApiService(_mockApiClient.Object, _mockCache.Object);
    }

    [Test]
    public async Task ThenWhenCacheHitTheApiIsNotCalledAndCachedResponseIsReturned()
    {
        var expectedResponse = new GetLevySummaryByHashedAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockCache
            .Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(CacheKey))
            .Returns(expectedResponse);

        var result = await _outerApiService.GetLevySummary(HashedAccountId);

        result.Should().Be(expectedResponse);
        _mockApiClient.Verify(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(It.IsAny<GetLevySummaryByHashedAccountIdRequest>()), Times.Never);
    }

    [Test]
    public async Task ThenWhenCacheMissTheOuterApiIsCalledAndResponseIsCachedAndReturned()
    {
        var expectedResponse = new GetLevySummaryByHashedAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(It.Is<GetLevySummaryByHashedAccountIdRequest>(r => r.HashedAccountId == HashedAccountId)))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevySummary(HashedAccountId);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse), Times.Once);
    }

    [Test]
    public async Task ThenWhenRefreshCacheIsTrueTheOuterApiIsCalledRegardlessOfCacheState()
    {
        var expectedResponse = new GetLevySummaryByHashedAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(It.IsAny<GetLevySummaryByHashedAccountIdRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevySummary(HashedAccountId, refreshCache: true);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Exists(It.IsAny<string>()), Times.Never);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse), Times.Once);
    }

    [Test]
    public async Task ThenWhenRefreshCacheIsTrueCacheIsNotRead()
    {
        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(It.IsAny<GetLevySummaryByHashedAccountIdRequest>()))
            .ReturnsAsync(new GetLevySummaryByHashedAccountIdResponse());

        await _outerApiService.GetLevySummary(HashedAccountId, refreshCache: true);

        _mockCache.Verify(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenDifferentHashedAccountIdsUseSeparateCacheKeys()
    {
        const string secondHashedAccountId = "XYZ789";

        var firstResponse = new GetLevySummaryByHashedAccountIdResponse
        {
            CurrentLevyFunds = 100M,
            TotalLevyDeclaredLast12Months = 200M
        };

        var secondResponse = new GetLevySummaryByHashedAccountIdResponse
        {
            CurrentLevyFunds = 300M,
            TotalLevyDeclaredLast12Months = 400M
        };

        _mockCache.Setup(x => x.Exists($"LevySummary_{HashedAccountId}")).Returns(true);
        _mockCache.Setup(x => x.Exists($"LevySummary_{secondHashedAccountId}")).Returns(true);
        _mockCache.Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>($"LevySummary_{HashedAccountId}")).Returns(firstResponse);
        _mockCache.Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>($"LevySummary_{secondHashedAccountId}")).Returns(secondResponse);

        var result1 = await _outerApiService.GetLevySummary(HashedAccountId);
        var result2 = await _outerApiService.GetLevySummary(secondHashedAccountId);

        result1.CurrentLevyFunds.Should().Be(100M);
        result2.CurrentLevyFunds.Should().Be(300M);
        result1.Should().NotBe(result2);
    }

    [Test]
    public async Task ThenWhenTheApiThrowsAnExceptionItPropagatesAndNothingIsCached()
    {
        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByHashedAccountIdResponse>(It.IsAny<GetLevySummaryByHashedAccountIdRequest>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var act = () => _outerApiService.GetLevySummary(HashedAccountId);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Service unavailable");
        _mockCache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}
