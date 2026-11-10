using AutoFixture.NUnit4;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLatestTransactionTypeTransactionIdsForAccount;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.LatestTransactionTypeTransactionIdsControllerTests;

[TestFixture]
public class WhenIGetLatestTransactionTypeTransactionIdsForAccount
{
    private const long AccountId = 12345;

    [Test, MoqAutoData]
    public async Task Then_Returns_The_Latest_Ids(
        GetLatestTransactionTypeTransactionIdsForAccountQueryResult mediatorResponse,
        [Frozen] Mock<IMediator> mediator,
        [Greedy] TransactionsController controller)
    {
        mediator
            .Setup(m => m.Send(It.Is<GetLatestTransactionTypeTransactionIdsForAccountQuery>(q => q.AccountId == AccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResponse);

        var result = await controller.GetLatestTransactionTypeTransactionIdsForAccount(AccountId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new LatestTransactionTypeTransactionIdsResponse
        {
            LatestDeclarationId = mediatorResponse.LatestDeclarationId,
            LatestPaymentId = mediatorResponse.LatestPaymentId,
            LatestExpiryId = mediatorResponse.LatestExpiryId
        });
    }
}
