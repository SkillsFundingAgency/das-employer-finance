using AutoFixture.NUnit4;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.OuterApiServiceTests;

[TestFixture]
internal class WhenGettingLevyProjectionsForDownload
{
    [Test, MoqAutoData]
    public async Task ThenTheDownloadProjectionsAreRequestedForTheAccount(long accountId,
        GetLevyProjectionsByAccountIdResponse expectedResponse,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] OuterApiService outerApiService,
        CancellationToken cancellationToken)
    {
        mockApiClient.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(
                It.Is<GetLevyProjectionsForDownloadByAccountIdRequest>(r => r.GetUrl == $"finance/levy/{accountId}/projections/download"),
                cancellationToken))
            .ReturnsAsync(expectedResponse);

        var result = await outerApiService.GetLevyProjectionsForDownload(accountId, cancellationToken);

        result.Should().Be(expectedResponse);
    }
}
