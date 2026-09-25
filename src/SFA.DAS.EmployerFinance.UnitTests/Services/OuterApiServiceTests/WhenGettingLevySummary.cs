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

    private const long AccountId = 123456789;
    private static string CacheKey => $"LevySummary_{AccountId}";

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
        var expectedResponse = new GetLevySummaryByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockCache
            .Setup(x => x.Get<GetLevySummaryByAccountIdResponse>(CacheKey))
            .Returns(expectedResponse);

        var result = await _outerApiService.GetLevySummary(AccountId);

        result.Should().Be(expectedResponse);
        _mockApiClient.Verify(x => x.Get<GetLevySummaryByAccountIdResponse>(It.IsAny<GetLevySummaryByAccountIdRequest>()), Times.Never);
    }

    [Test]
    public async Task ThenWhenCacheMissTheOuterApiIsCalledAndResponseIsCachedAndReturned()
    {
        var expectedResponse = new GetLevySummaryByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByAccountIdResponse>(It.Is<GetLevySummaryByAccountIdRequest>(r => r.AccountId == AccountId)))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevySummary(AccountId);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse, TimeSpan.FromHours(24)), Times.Once);
    }

    [Test]
    public async Task ThenWhenRefreshCacheIsTrueTheOuterApiIsCalledRegardlessOfCacheState()
    {
        var expectedResponse = new GetLevySummaryByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByAccountIdResponse>(It.IsAny<GetLevySummaryByAccountIdRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevySummary(AccountId, refreshCache: true);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Exists(It.IsAny<string>()), Times.Never);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse, TimeSpan.FromHours(24)), Times.Once);
    }

    [Test]
    public async Task ThenWhenRefreshCacheIsTrueCacheIsNotRead()
    {
        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByAccountIdResponse>(It.IsAny<GetLevySummaryByAccountIdRequest>()))
            .ReturnsAsync(new GetLevySummaryByAccountIdResponse());

        await _outerApiService.GetLevySummary(AccountId, refreshCache: true);

        _mockCache.Verify(x => x.Get<GetLevySummaryByAccountIdResponse>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenDifferentHashedAccountIdsUseSeparateCacheKeys()
    {
        const long secondAccountId = 987654321;

        var firstResponse = new GetLevySummaryByAccountIdResponse
        {
            CurrentLevyFunds = 100M,
            TotalLevyDeclaredLast12Months = 200M,
            TotalCommittedLearnerCosts = 150M,
            TotalCommittedTransfersCosts = 100M,
            TotalLevyExpiredLast12Months = 50M,
            TotalLevySpentLast12Months = 100M
        };

        var secondResponse = new GetLevySummaryByAccountIdResponse
        {
            CurrentLevyFunds = 300M,
            TotalLevyDeclaredLast12Months = 400M,
            TotalCommittedLearnerCosts = 250M,
            TotalCommittedTransfersCosts = 200M,
            TotalLevyExpiredLast12Months = 150M,
            TotalLevySpentLast12Months = 300M
        };

        _mockCache.Setup(x => x.Exists($"LevySummary_{AccountId}")).Returns(true);
        _mockCache.Setup(x => x.Exists($"LevySummary_{secondAccountId}")).Returns(true);
        _mockCache.Setup(x => x.Get<GetLevySummaryByAccountIdResponse>($"LevySummary_{AccountId}")).Returns(firstResponse);
        _mockCache.Setup(x => x.Get<GetLevySummaryByAccountIdResponse>($"LevySummary_{secondAccountId}")).Returns(secondResponse);

        var result1 = await _outerApiService.GetLevySummary(AccountId);
        var result2 = await _outerApiService.GetLevySummary(secondAccountId);

        result1.CurrentLevyFunds.Should().Be(100M);
        result2.CurrentLevyFunds.Should().Be(300M);
        result1.TotalLevyDeclaredLast12Months.Should().Be(200M);
        result2.TotalLevyDeclaredLast12Months.Should().Be(400M);    
        result1.TotalLevyDeclaredLast12Months.Should().NotBe(result2.TotalLevyDeclaredLast12Months);
        result2.TotalLevyDeclaredLast12Months.Should().NotBe(result1.TotalLevyDeclaredLast12Months);
        result1.TotalLevySpentLast12Months.Should().Be(100M);
        result2.TotalLevySpentLast12Months.Should().Be(300M);
        result1.TotalLevyExpiredLast12Months.Should().Be(50M);
        result2.TotalLevyExpiredLast12Months.Should().Be(150M);
        result1.TotalLevyExpiredLast12Months.Should().NotBe(result2.TotalLevyExpiredLast12Months);
        result1.TotalCommittedLearnerCosts.Should().Be(150M);
        result2.TotalCommittedLearnerCosts.Should().Be(250M);
        result1.Should().NotBe(result2);
    }

    [Test]
    public async Task ThenWhenTheApiThrowsAnExceptionItPropagatesAndNothingIsCached()
    {
        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevySummaryByAccountIdResponse>(It.IsAny<GetLevySummaryByAccountIdRequest>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var act = () => _outerApiService.GetLevySummary(AccountId);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Service unavailable");
        _mockCache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}