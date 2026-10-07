using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Services;
using System.Net.Http;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.OuterApiServiceTests;

[TestFixture]
internal class WhenGettingLevyProjections
{
    private Mock<IOuterApiClient> _mockApiClient;
    private Mock<IInProcessCache> _mockCache;
    private Mock<ICacheInvalidationRule<GetLevyProjectionsByAccountIdResponse>> _mockInvalidationRule;
    private Mock<ILogger<OuterApiService>> _mockLogger;
    private OuterApiService _outerApiService;

    private const long AccountId = 123456789;
    private const int Months = 6;

    private static readonly TimeSpan CacheDuration30Days = TimeSpan.FromDays(30);

    private static string CacheKey => $"LevyProjections_{AccountId}_{Months}";

    [SetUp]
    public void Arrange()
    {
        _mockApiClient = new Mock<IOuterApiClient>();
        _mockCache = new Mock<IInProcessCache>();
        _mockInvalidationRule = new Mock<ICacheInvalidationRule<GetLevyProjectionsByAccountIdResponse>>();
        _mockLogger = new Mock<ILogger<OuterApiService>>();

        _outerApiService = new OuterApiService(
            _mockApiClient.Object,
            _mockCache.Object,
            [_mockInvalidationRule.Object], 
            _mockLogger.Object);
    }

    [Test]
    public async Task ThenWhenCacheHitAndNoRuleInvalidatesTheCachedResponseIsReturned()
    {
        var expectedResponse = new GetLevyProjectionsByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockCache
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(CacheKey))
            .Returns(expectedResponse);

        // Rule says: do not invalidate
        _mockInvalidationRule
            .Setup(x => x.ShouldInvalidateAsync(expectedResponse, It.Is<CacheInvalidationContext>(c => c.AccountId == AccountId)))
            .ReturnsAsync(false);

        var result = await _outerApiService.GetLevyProjections(AccountId, Months);

        result.Should().Be(expectedResponse);
        _mockApiClient.Verify(
            x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()),
            Times.Never);
    }

    [Test]
    public async Task ThenWhenCacheHitAndRuleInvalidatesTheApiIsCalledAndResponseIsCachedAndReturned()
    {
        var cachedResponse = new GetLevyProjectionsByAccountIdResponse();
        var freshResponse = new GetLevyProjectionsByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockCache
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(CacheKey))
            .Returns(cachedResponse);

        // Rule says: invalidate — declaration date has changed
        _mockInvalidationRule
            .Setup(x => x.ShouldInvalidateAsync(cachedResponse, It.Is<CacheInvalidationContext>(c => c.AccountId == AccountId)))
            .ReturnsAsync(true);

        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(
                It.Is<GetLevyProjectionsByAccountIdRequest>(r => r.AccountId == AccountId && r.Months == Months)))
            .ReturnsAsync(freshResponse);

        var result = await _outerApiService.GetLevyProjections(AccountId, Months);

        result.Should().Be(freshResponse);
        _mockCache.Verify(x => x.Set(CacheKey, freshResponse, CacheDuration30Days), Times.Once);
    }

    [Test]
    public async Task ThenWhenCacheMissRulesAreNotEvaluatedAndApiIsCalledAndResponseIsCachedAndReturned()
    {
        var expectedResponse = new GetLevyProjectionsByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(
                It.Is<GetLevyProjectionsByAccountIdRequest>(r => r.AccountId == AccountId && r.Months == Months)))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevyProjections(AccountId, Months);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse, CacheDuration30Days), Times.Once);

        // Rules should not be consulted when there is nothing in the cache
        _mockInvalidationRule.Verify(
            x => x.ShouldInvalidateAsync(It.IsAny<GetLevyProjectionsByAccountIdResponse>(), It.IsAny<CacheInvalidationContext>()),
            Times.Never);
    }

    [Test]
    public async Task ThenWhenFirstRuleInvalidatesSubsequentRulesAreNotEvaluated()
    {
        var cachedResponse = new GetLevyProjectionsByAccountIdResponse();
        var secondRule = new Mock<ICacheInvalidationRule<GetLevyProjectionsByAccountIdResponse>>();

        // Rebuild the service with two rules
        _outerApiService = new OuterApiService(
            _mockApiClient.Object,
            _mockCache.Object,
            [_mockInvalidationRule.Object, secondRule.Object],
            _mockLogger.Object);

        _mockCache.Setup(x => x.Exists(CacheKey)).Returns(true);
        _mockCache.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(CacheKey)).Returns(cachedResponse);

        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse());

        // First rule fires
        _mockInvalidationRule
            .Setup(x => x.ShouldInvalidateAsync(cachedResponse, It.IsAny<CacheInvalidationContext>()))
            .ReturnsAsync(true);

        await _outerApiService.GetLevyProjections(AccountId, Months);

        // Second rule should never be reached
        secondRule.Verify(
            x => x.ShouldInvalidateAsync(It.IsAny<GetLevyProjectionsByAccountIdResponse>(), It.IsAny<CacheInvalidationContext>()),
            Times.Never);
    }

    [Test]
    public async Task ThenDifferentAccountIdsUseSeparateCacheKeys()
    {
        const long secondAccountId = 987654321;

        var firstResponse = new GetLevyProjectionsByAccountIdResponse
        {
            Projections = [new GetLevyProjectionsByAccountIdResponse.MonthlyBreakdown { LevyIn = 1000M, CalendarMonthName = "August", CalendarPeriodMonth = 8, CalendarPeriodYear = 2026 }]
        };
        var secondResponse = new GetLevyProjectionsByAccountIdResponse
        {
            Projections = [new GetLevyProjectionsByAccountIdResponse.MonthlyBreakdown { LevyIn = 2000M, CalendarMonthName = "September", CalendarPeriodMonth = 9, CalendarPeriodYear = 2026 }]
        };

        _mockCache.Setup(x => x.Exists($"LevyProjections_{AccountId}_{Months}")).Returns(true);
        _mockCache.Setup(x => x.Exists($"LevyProjections_{secondAccountId}_{Months}")).Returns(true);
        _mockCache.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>($"LevyProjections_{AccountId}_{Months}")).Returns(firstResponse);
        _mockCache.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>($"LevyProjections_{secondAccountId}_{Months}")).Returns(secondResponse);

        // Neither cached entry needs invalidating
        _mockInvalidationRule
            .Setup(x => x.ShouldInvalidateAsync(It.IsAny<GetLevyProjectionsByAccountIdResponse>(), It.IsAny<CacheInvalidationContext>()))
            .ReturnsAsync(false);

        var result1 = await _outerApiService.GetLevyProjections(AccountId, Months);
        var result2 = await _outerApiService.GetLevyProjections(secondAccountId, Months);

        result1.Should().NotBeSameAs(result2);
        result1.Projections[0].LevyIn.Should().Be(1000M);
        result2.Projections[0].LevyIn.Should().Be(2000M);

        _mockApiClient.Verify(
            x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()),
            Times.Never);

        _mockCache.Verify(x => x.Exists($"LevyProjections_{AccountId}_{Months}"), Times.Once);
        _mockCache.Verify(x => x.Exists($"LevyProjections_{secondAccountId}_{Months}"), Times.Once);
    }

    [Test]
    public async Task ThenWhenTheApiThrowsAnExceptionItPropagatesAndNothingIsCached()
    {
        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var result = await _outerApiService.GetLevyProjections(AccountId);

        result.Should().BeEquivalentTo(new GetLevyProjectionsByAccountIdResponse());
    }
}