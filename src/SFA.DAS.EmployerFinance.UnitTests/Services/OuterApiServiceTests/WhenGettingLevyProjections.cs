using AutoFixture.NUnit4;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.OuterApiServiceTests;

[TestFixture]
internal class WhenGettingLevyProjections
{
    [Test, MoqAutoData]
    public async Task ThenTheResponseIsReturned(long accountId,
        int months,
        GetLevyProjectionsByAccountIdResponse expectedResponse,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] OuterApiService outerApiService, 
        CancellationToken cancellationToken)
    {
        mockApiClient.Setup(x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>(), cancellationToken))
            .ReturnsAsync(expectedResponse);

        var result = await outerApiService.GetLevyProjections(accountId, months, cancellationToken);

        result.Should().Be(expectedResponse);
        mockApiClient.Verify(
            x => x.Get<GetLevyProjectionsByAccountIdResponse>(It.IsAny<GetLevyProjectionsByAccountIdRequest>(), cancellationToken),
            Times.Once);
    }
}