using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Account;

namespace SFA.DAS.EmployerFinance.Jobs.UnitTests.ScheduledJobs;

[TestFixture]
[Parallelizable]
public class ExpireFundsJobTests : ExpireFundsJobTestsFixture
{
    [Test]
    [TestCase(0, Description = "No accounts")]
    [TestCase(1, Description = "Single account")]
    [TestCase(99, Description = "Multiple accounts")]
    public async Task Run_WhenRunningJob_ThenShouldSendCommand(int numberOfAccounts)
    {
        var fixture = new ExpireFundsJobTestsFixture();
        fixture.SetupAccounts(numberOfAccounts);
        
        await fixture.Run();
        
        fixture.MessageSession.Verify(s =>
                s.Send(
                    It.IsAny<ExpireAccountFundsCommand>(),
                    It.IsAny<SendOptions>()),
            Times.Exactly(numberOfAccounts)
        );
    }
}

public class ExpireFundsJobTestsFixture
{
    public Mock<IMessageSession> MessageSession { get; set; }
    public Mock<ICurrentDateTime> CurrentDateTime { get; set; }
    public Mock<IEmployerAccountRepository> EmployerAccountRepository;
    public ExpireFundsJob Job { get; set; }
    private readonly IFixture _fixture;

    public ExpireFundsJobTestsFixture()
    {
        _fixture = new Fixture();

        MessageSession = new Mock<IMessageSession>();
        CurrentDateTime = new Mock<ICurrentDateTime>();
        EmployerAccountRepository = new Mock<IEmployerAccountRepository>();

        Job = new ExpireFundsJob(MessageSession.Object, CurrentDateTime.Object, EmployerAccountRepository.Object);
    }

    public Task Run()
    {
        return Job.Run(null, Mock.Of<ILogger<ExpireFundsJob>>());
    }

    internal void SetupAccounts(int numberOfAccounts)
    {
        _fixture.Customize(new AutoMoqCustomization());

        var accounts = _fixture
            .Build<Account>().Without(acc => acc.AccountLegalEntities)
            .CreateMany(numberOfAccounts);

        EmployerAccountRepository.Setup(x => x.GetAll()).ReturnsAsync(accounts.ToList());
    }
}