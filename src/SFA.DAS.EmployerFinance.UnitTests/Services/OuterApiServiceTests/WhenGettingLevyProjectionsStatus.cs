using AutoFixture.NUnit4;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.OuterApiServiceTests;

[TestFixture]
internal class WhenGettingLevyProjectionsStatus
{
    [Test, MoqAutoData]
    public async Task ThenTheResponseIsReturned(long accountId,
        GetLevyProjectionsStatusByAccountIdResponse expectedResponse,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] OuterApiService outerApiService,
        CancellationToken cancellationToken)
    {
        mockApiClient.Setup(x => x.Get<GetLevyProjectionsStatusByAccountIdResponse>(
                It.Is<GetLevyProjectionsStatusByAccountIdRequest>(r => r.GetUrl == $"finance/levy/{accountId}/projections/status"), cancellationToken))
            .ReturnsAsync(expectedResponse);

        var result = await outerApiService.GetLevyProjectionsStatus(accountId, cancellationToken);

        result.Should().Be(expectedResponse);
    }
}
