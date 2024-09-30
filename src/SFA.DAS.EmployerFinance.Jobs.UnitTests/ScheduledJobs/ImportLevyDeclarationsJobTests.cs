using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Paye;
using SFA.DAS.Testing;

namespace SFA.DAS.EmployerFinance.Jobs.UnitTests.ScheduledJobs;

[TestFixture]
[Parallelizable]
public class ImportLevyDeclarationsJobTests : FluentTest<ImportLevyDeclarationsJobTestsFixture>
{
    [Test]
    [TestCase(0, Description = "No accounts")]
    [TestCase(1, Description = "Single account")]
    [TestCase(99, Description = "Multiple accounts")]
    public async Task Only_Processes_PAYE_Schemes_Added_Using_GovernmentGateway(int numberOfAccounts)
    {
        var fixture = new ImportLevyDeclarationsJobTestsFixture();
        fixture.SetupAccounts(numberOfAccounts);

        await fixture.Run();

        fixture.VerifyGovGatewayCalls(numberOfAccounts);
    }

    [Test]
    [TestCase(0, 0, 0, Description = "No accounts")]
    [TestCase(1, 1, 1, Description = "Single account, single PAYE")]
    [TestCase(99, 1, 99, Description = "Multiple accounts, single PAYE")]
    [TestCase(1, 5, 5, Description = "Single account, multiple PAYE")]
    [TestCase(99, 2, 198, Description = "Multiple accounts, multiple PAYE")]
    public async Task Run_WhenRunningJob_ThenShouldSendCommand_PerAccounPaye(int numberOfAccounts, int numberOfPayeSchemes, int expectedNumberOfMessages)
    {
        var fixture = new ImportLevyDeclarationsJobTestsFixture();
        fixture.SetupAccounts(numberOfAccounts);
        fixture.SetupPaye(numberOfPayeSchemes);
        
        await fixture.Run();
        
        fixture.VerifyMessagesSent(expectedNumberOfMessages);
    }
}

public class ImportLevyDeclarationsJobTestsFixture
{
    private readonly Mock<IMessageSession> _messageSession;
    private readonly Mock<IEmployerAccountRepository> _employerAccountRepository;
    private readonly Mock<IPayeRepository> _payeRepository;
    private readonly ImportLevyDeclarationsJob _job;

    private readonly IFixture Fixture;

    private IEnumerable<Account> Accounts = new List<Account>();

    public ImportLevyDeclarationsJobTestsFixture()
    {
        Fixture = new Fixture();

        _messageSession = new Mock<IMessageSession>();
        _employerAccountRepository = new Mock<IEmployerAccountRepository>();
        _payeRepository = new Mock<IPayeRepository>();

        _job = new ImportLevyDeclarationsJob(_messageSession.Object, _employerAccountRepository.Object, _payeRepository.Object);
    }

    public Task Run()
    {
        return _job.Run(null, Mock.Of<ILogger<ImportLevyDeclarationsJob>>());
    }

    internal void SetupAccounts(int numberOfAccounts)
    {
        Fixture.Customize(new AutoMoqCustomization());

        Accounts = Fixture
            .Build<Account>().Without(acc => acc.AccountLegalEntities)
            .CreateMany(numberOfAccounts);

        _employerAccountRepository.Setup(x => x.GetAll()).ReturnsAsync(Accounts.ToList());
    }

    internal void SetupPaye(int numberOfPayeSchemes)
    {
        Accounts
            .ToList()
            .ForEach(acc => SetupAccountPayes(acc.Id, numberOfPayeSchemes));
    }

    internal void VerifyMessagesSent(int numberOfMessages)
    {
        _messageSession.Verify(x => x.Send(It.IsAny<ImportAccountLevyDeclarationsCommand>(), It.IsAny<SendOptions>()), Times.Exactly(numberOfMessages));
    }

    internal void VerifyGovGatewayCalls(int numberOfAccounts)
    {
        _payeRepository
            .Verify(
                m =>
                    m.GetGovernmentGatewayOnlySchemesByEmployerId(
                        It.Is<long>(accountId => AccountIdInRepositoryResults(accountId))),
                Times.Exactly(Accounts.Count()));
    }

    private bool AccountIdInRepositoryResults(long accountId)
    {
        return
            Accounts
                .Any(acc => acc.Id == accountId);
    }

    private void SetupAccountPayes(long accoundId, int numberOfPayeSchemes)
    {
        var schemeList = Fixture
            .Build<Paye>()
            .With(paye => paye.AccountId, accoundId)
            .CreateMany(numberOfPayeSchemes)
            .ToList();

        var payes = Fixture
            .Build<PayeSchemes>()
            .With(p => p.SchemesList, schemeList)
            .Create();


        _payeRepository.Setup(x => x.GetGovernmentGatewayOnlySchemesByEmployerId(accoundId)).ReturnsAsync(payes);
    }
}