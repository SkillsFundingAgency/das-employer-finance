using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services;
using System.Net.Http;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.OuterApiServiceTests;

[TestFixture]
internal class WhenGettingLevyProjections
{
    private Mock<IOuterApiClient> _mockApiClient;
    private Mock<IInProcessCache> _mockCache;
    private OuterApiService _outerApiService;

    private const long AccountId = 123456789;
    private const int Months = 12;
    private static string CacheKey => $"LevyProjections_{AccountId}_{Months}";

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
        var expectedResponse = new GetLevyProjectionsByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockCache
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(CacheKey))
            .Returns(expectedResponse);

        var result = await _outerApiService.GetLevyProjections(AccountId);

        result.Should().Be(expectedResponse);
        _mockApiClient.Verify(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()), Times.Never);
    }

    [Test]
    public async Task ThenWhenCacheMissTheOuterApiIsCalledAndResponseIsCachedAndReturned()
    {
        var expectedResponse = new GetLevyProjectionsByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(false);

        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.Is<GetLevyProjectionsByAccountIdRequest>(r => r.AccountId == AccountId && r.Months == Months)))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevyProjections(AccountId, Months);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse, TimeSpan.FromHours(24)), Times.Once);
    }

    [Test]
    public async Task ThenWhenRefreshCacheIsTrueTheOuterApiIsCalledRegardlessOfCacheState()
    {
        var expectedResponse = new GetLevyProjectionsByAccountIdResponse();

        _mockCache
            .Setup(x => x.Exists(CacheKey))
            .Returns(true);

        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _outerApiService.GetLevyProjections(AccountId, Months, refreshCache: true);

        result.Should().Be(expectedResponse);
        _mockCache.Verify(x => x.Exists(It.IsAny<string>()), Times.Never);
        _mockCache.Verify(x => x.Set(CacheKey, expectedResponse, TimeSpan.FromHours(24)), Times.Once);
    }

    [Test]
    public async Task ThenWhenRefreshCacheIsTrueCacheIsNotRead()
    {
        _mockApiClient
            .Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse());

        await _outerApiService.GetLevyProjections(AccountId, Months, refreshCache: true);

        _mockCache.Verify(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenDifferentHashedAccountIdsUseSeparateCacheKeys()
    {
        const long secondAccountId = 987654321;

        var firstResponse = new GetLevyProjectionsByAccountIdResponse
        {
            Projections =
            [
                new() { LevyIn = 1000M, CalendarMonthName = "August", CalendarPeriodMonth = 8, CalendarPeriodYear = 2026 }
            ]
        };

        var secondResponse = new GetLevyProjectionsByAccountIdResponse
        {
            Projections =
            [
                new() { LevyIn = 2000M, CalendarMonthName = "September", CalendarPeriodMonth = 9, CalendarPeriodYear = 2026 }
            ]
        };

        _mockCache.Setup(x => x.Exists($"LevyProjections_{AccountId}_{Months}")).Returns(true);
        _mockCache.Setup(x => x.Exists($"LevyProjections_{secondAccountId}_{Months}")).Returns(true);
        _mockCache.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>($"LevyProjections_{AccountId}_{Months}")).Returns(firstResponse);
        _mockCache.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>($"LevyProjections_{secondAccountId}_{Months}")).Returns(secondResponse);

        var result1 = await _outerApiService.GetLevyProjections(AccountId, Months);
        var result2 = await _outerApiService.GetLevyProjections(secondAccountId, Months);

        // Each account returns its own cached response
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().NotBeSameAs(result2);

        // Correct projections returned per account
        result1.Projections.Should().HaveCount(1);
        result1.Projections[0].LevyIn.Should().Be(1000M);
        result1.Projections[0].CalendarMonthName.Should().Be("August");

        result2.Projections.Should().HaveCount(1);
        result2.Projections[0].LevyIn.Should().Be(2000M);
        result2.Projections[0].CalendarMonthName.Should().Be("September");

        // API was never called — both came from cache
        _mockApiClient.Verify(
            x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>()),
            Times.Never);

        // Each cache key was checked independently
        _mockCache.Verify(x => x.Exists($"LevyProjections_{AccountId}_{Months}"), Times.Once);
        _mockCache.Verify(x => x.Exists($"LevyProjections_{secondAccountId}_{Months}"), Times.Once);

        // Each cache key was read independently
        _mockCache.Verify(x => x.Get<GetLevyProjectionsByAccountIdResponse>($"LevyProjections_{AccountId}_{Months}"), Times.Once);
        _mockCache.Verify(x => x.Get<GetLevyProjectionsByAccountIdResponse>($"LevyProjections_{secondAccountId}_{Months}"), Times.Once);
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

        var act = () => _outerApiService.GetLevyProjections(AccountId);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Service unavailable");
        _mockCache.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}
