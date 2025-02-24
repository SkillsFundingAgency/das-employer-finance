using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests;

public class WhenIGetAllTransactionsFrom
{
    [Test, MoqAutoData]
    public async Task Then_Gets_All_Transactions_From_Date_From_Orchestrator(
           string hashedAccountId,
           Transactions transactions,
           [Frozen] Mock<AccountTransactionsOrchestrator> orchestrator,
           [Greedy] AccountTransactionsController controller)
    {
        orchestrator
               .Setup(x => x.GetAccountTransactions(
                   It.IsAny<string>(),
                   It.IsAny<int>(),
                   It.IsAny<int>(),
                   It.IsAny<bool>()
                   )).ReturnsAsync(transactions);

        var controllerResult = await controller.GetAllTransactionsFrom(hashedAccountId, 2022, 1) as ObjectResult;

        controllerResult.Should().NotBeNull();
        var model = controllerResult.Value as Transactions;

        model.Should().NotBeNull();
        model.Should().BeEquivalentTo(transactions);
    }
}
