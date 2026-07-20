using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.HmrcLevy;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.NServiceBus.Testing.Services;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshEmployerLevyDataTests;

[TestFixture]
public class WhenPublishingAccountLevyRefreshEvents
{
    [Test]
    public async Task ThenOneAccountLevelEventIsPublishedWithHistoricalLastDeclarationWhenNoNewLevy()
    {
        const long accountId = 100;
        const string payeRef = "AAA/11111";
        var historicalDate = new DateTime(2024, 3, 15);
        var runDate = new DateTime(2026, 6, 1);

        var fixture = new AccountLevyRefreshEventsFixture();
        fixture.SetAccountId(accountId);
        fixture.SetRunDate(runDate);
        fixture.SetLastPositiveNetDeclarationForAccount(accountId, new DasDeclaration { SubmissionDate = historicalDate });
        fixture.SetEmployerLevyData(new List<EmployerLevyData>
        {
            new()
            {
                EmpRef = payeRef,
                Declarations = new DasDeclarations
                {
                    Declarations = new List<DasDeclaration>()
                }
            }
        });

        await fixture.Handle();

        var accountEvent = fixture.GetPublishedEvent();
        accountEvent.Should().NotBeNull();
        accountEvent.AccountId.Should().Be(accountId);
        accountEvent.PayeRef.Should().BeNull();
        accountEvent.LevyImported.Should().BeFalse();
        accountEvent.LevyTransactionValue.Should().Be(0m);
        accountEvent.LastLevyDeclarationDate.Should().Be(historicalDate);
    }

    [Test]
    public async Task ThenLastDeclarationDateIsRunDateWhenPositiveLevyIsProcessed()
    {
        const long accountId = 100;
        const string payeRef = "AAA/11111";
        var runDate = new DateTime(2026, 6, 1, 14, 30, 0);

        var fixture = new AccountLevyRefreshEventsFixture();
        fixture.SetAccountId(accountId);
        fixture.SetRunDate(runDate);
        fixture.SetEmployerLevyData(new List<EmployerLevyData>
        {
            new()
            {
                EmpRef = payeRef,
                Declarations = new DasDeclarations
                {
                    Declarations = new List<DasDeclaration>
                    {
                        new() { LevyDueYtd = 2000, PayrollYear = "2024", PayrollMonth = 3, SubmissionId = 1 }
                    }
                }
            }
        });
        fixture.SetupProcessDeclarations(accountId, payeRef, 50m);

        await fixture.Handle();

        var accountEvent = fixture.GetPublishedEvent();
        accountEvent.LevyImported.Should().BeTrue();
        accountEvent.LevyTransactionValue.Should().Be(50m);
        accountEvent.LastLevyDeclarationDate.Should().Be(runDate.Date);
        fixture.VerifyAccountLookupWasNotCalled();
    }

    [Test]
    public async Task ThenHistoricalLastDeclarationIsUsedWhenDeclarationsImportedWithZeroValue()
    {
        const long accountId = 100;
        const string payeRef = "AAA/11111";
        var historicalDate = new DateTime(2024, 1, 10);
        var runDate = new DateTime(2026, 6, 1);

        var fixture = new AccountLevyRefreshEventsFixture();
        fixture.SetAccountId(accountId);
        fixture.SetRunDate(runDate);
        fixture.SetLastPositiveNetDeclarationForAccount(accountId, new DasDeclaration { SubmissionDate = historicalDate });
        fixture.SetEmployerLevyData(new List<EmployerLevyData>
        {
            new()
            {
                EmpRef = payeRef,
                Declarations = new DasDeclarations
                {
                    Declarations = new List<DasDeclaration>
                    {
                        new() { LevyDueYtd = 0, PayrollYear = "2024", PayrollMonth = 3, SubmissionId = 1 }
                    }
                }
            }
        });
        fixture.SetupProcessDeclarations(accountId, payeRef, 0m);

        await fixture.Handle();

        var accountEvent = fixture.GetPublishedEvent();
        accountEvent.LevyImported.Should().BeTrue();
        accountEvent.LevyTransactionValue.Should().Be(0m);
        accountEvent.LastLevyDeclarationDate.Should().Be(historicalDate);
    }

    [Test]
    public async Task ThenNoEventsArePublishedWhenEmployerLevyDataIsEmpty()
    {
        var fixture = new AccountLevyRefreshEventsFixture();
        fixture.SetAccountId(200);
        fixture.SetEmployerLevyData(new List<EmployerLevyData>());

        await fixture.Handle();

        fixture.GetPublishedEvent().Should().BeNull();
    }
}

public class AccountLevyRefreshEventsFixture
{
    private readonly TestableEventPublisher _eventPublisher;
    private readonly Mock<IDasLevyRepository> _dasLevyRepository;
    private readonly Mock<ICurrentDateTime> _currentDateTime;
    private readonly RefreshEmployerLevyDataCommandHandler _handler;
    private long _accountId;
    private ICollection<EmployerLevyData> _employerLevyData = new List<EmployerLevyData>();

    public AccountLevyRefreshEventsFixture()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _eventPublisher = new TestableEventPublisher();
        _currentDateTime = new Mock<ICurrentDateTime>();

        var validator = new Mock<IValidator<RefreshEmployerLevyDataCommand>>();
        validator.Setup(x => x.Validate(It.IsAny<RefreshEmployerLevyDataCommand>())).Returns(new ValidationResult());

        var hmrcDateService = new Mock<IHmrcDateService>();
        hmrcDateService.Setup(x => x.IsSubmissionForFuturePeriod(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<DateTime>())).Returns(false);

        var levyImportCleanerStrategy = new LevyImportCleanerStrategy(
            _dasLevyRepository.Object,
            hmrcDateService.Object,
            Mock.Of<ILogger<LevyImportCleanerStrategy>>(),
            _currentDateTime.Object);

        _handler = new RefreshEmployerLevyDataCommandHandler(
            validator.Object,
            _dasLevyRepository.Object,
            levyImportCleanerStrategy,
            _eventPublisher,
            _currentDateTime.Object,
            Mock.Of<ILogger<RefreshEmployerLevyDataCommandHandler>>());
    }

    public AccountLevyRefreshEventsFixture SetAccountId(long accountId)
    {
        _accountId = accountId;
        return this;
    }

    public AccountLevyRefreshEventsFixture SetRunDate(DateTime runDate)
    {
        _currentDateTime.Setup(x => x.Now).Returns(runDate);
        return this;
    }

    public AccountLevyRefreshEventsFixture SetLastPositiveNetDeclarationForAccount(long accountId, DasDeclaration declaration)
    {
        _dasLevyRepository.Setup(x => x.GetLastPositiveNetDeclarationForAccount(accountId)).ReturnsAsync(declaration);
        return this;
    }

    public AccountLevyRefreshEventsFixture SetEmployerLevyData(ICollection<EmployerLevyData> employerLevyData)
    {
        _employerLevyData = employerLevyData;
        return this;
    }

    public AccountLevyRefreshEventsFixture SetupProcessDeclarations(long accountId, string empRef, decimal amount)
    {
        _dasLevyRepository.Setup(x => x.ProcessDeclarations(accountId, empRef)).ReturnsAsync(amount);
        return this;
    }

    public Task Handle() =>
        _handler.Handle(new RefreshEmployerLevyDataCommand
        {
            AccountId = _accountId,
            EmployerLevyData = _employerLevyData
        }, CancellationToken.None);

    public RefreshEmployerLevyDataCompletedEvent GetPublishedEvent() =>
        _eventPublisher.Events.OfType<RefreshEmployerLevyDataCompletedEvent>().SingleOrDefault();

    public void VerifyAccountLookupWasNotCalled()
    {
        _dasLevyRepository.Verify(x => x.GetLastPositiveNetDeclarationForAccount(It.IsAny<long>()), Times.Never);
    }
}
