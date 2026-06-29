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
public class WhenPublishingPerPayeRefreshEvents
{
    [Test]
    public async Task ThenOneEventIsPublishedPerPayeSchemeWithLastPositiveDeclarationDate()
    {
        const long accountId = 100;
        const string payeRefOne = "AAA/11111";
        const string payeRefTwo = "BBB/22222";
        var lastDeclarationDate = new DateTime(2024, 3, 15);

        var fixture = new PerPayeRefreshEventsFixture();
        fixture.SetAccountId(accountId);
        fixture.SetIsSubmissionForFuturePeriod(false);
        fixture.SetLastPositiveNetDeclaration(payeRefOne, new DasDeclaration { SubmissionDate = lastDeclarationDate });
        fixture.SetLastPositiveNetDeclaration(payeRefTwo, null);
        fixture.SetEmployerLevyData(new List<EmployerLevyData>
        {
            new()
            {
                EmpRef = payeRefOne,
                Declarations = new DasDeclarations
                {
                    Declarations = new List<DasDeclaration>
                    {
                        new() { LevyDueYtd = 2000, PayrollYear = "2024", PayrollMonth = 3, SubmissionId = 1 }
                    }
                }
            },
            new()
            {
                EmpRef = payeRefTwo,
                Declarations = new DasDeclarations
                {
                    Declarations = new List<DasDeclaration>()
                }
            }
        });

        fixture.SetupProcessDeclarations(accountId, payeRefOne, 50m);

        await fixture.Handle();

        var events = fixture.GetPublishedEvents();
        events.Count.Should().Be(2);
        events.Should().Contain(e => e.PayeRef == payeRefOne && e.LastLevyDeclarationDate == lastDeclarationDate && e.LevyImported);
        events.Should().Contain(e => e.PayeRef == payeRefTwo && e.LastLevyDeclarationDate == null && !e.LevyImported);
    }

    [Test]
    public async Task ThenNoEventsArePublishedWhenEmployerLevyDataIsEmpty()
    {
        var fixture = new PerPayeRefreshEventsFixture();
        fixture.SetAccountId(200);
        fixture.SetEmployerLevyData(new List<EmployerLevyData>());

        await fixture.Handle();

        fixture.GetPublishedEvents().Should().BeEmpty();
    }
}

public class PerPayeRefreshEventsFixture
{
    private readonly TestableEventPublisher _eventPublisher;
    private readonly Mock<IDasLevyRepository> _dasLevyRepository;
    private readonly RefreshEmployerLevyDataCommandHandler _handler;
    private long _accountId;
    private ICollection<EmployerLevyData> _employerLevyData = new List<EmployerLevyData>();

    public PerPayeRefreshEventsFixture()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _eventPublisher = new TestableEventPublisher();

        var validator = new Mock<IValidator<RefreshEmployerLevyDataCommand>>();
        validator.Setup(x => x.Validate(It.IsAny<RefreshEmployerLevyDataCommand>())).Returns(new ValidationResult());

        var hmrcDateService = new Mock<IHmrcDateService>();
        hmrcDateService.Setup(x => x.IsSubmissionForFuturePeriod(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<DateTime>())).Returns(false);

        var currentDateTime = new Mock<ICurrentDateTime>();
        currentDateTime.Setup(cdt => cdt.Now).Returns(() => DateTime.UtcNow);

        var levyImportCleanerStrategy = new LevyImportCleanerStrategy(
            _dasLevyRepository.Object,
            hmrcDateService.Object,
            Mock.Of<ILogger<LevyImportCleanerStrategy>>(),
            currentDateTime.Object);

        _handler = new RefreshEmployerLevyDataCommandHandler(
            validator.Object,
            _dasLevyRepository.Object,
            levyImportCleanerStrategy,
            _eventPublisher,
            Mock.Of<ILogger<RefreshEmployerLevyDataCommandHandler>>());
    }

    public PerPayeRefreshEventsFixture SetAccountId(long accountId)
    {
        _accountId = accountId;
        return this;
    }

    public PerPayeRefreshEventsFixture SetIsSubmissionForFuturePeriod(bool result)
    {
        return this;
    }

    public PerPayeRefreshEventsFixture SetLastPositiveNetDeclaration(string empRef, DasDeclaration declaration)
    {
        _dasLevyRepository.Setup(x => x.GetLastPositiveNetDeclarationForScheme(empRef)).ReturnsAsync(declaration);
        _dasLevyRepository.Setup(x => x.GetLastSubmissionForScheme(empRef)).ReturnsAsync(declaration);
        return this;
    }

    public PerPayeRefreshEventsFixture SetEmployerLevyData(ICollection<EmployerLevyData> employerLevyData)
    {
        _employerLevyData = employerLevyData;
        return this;
    }

    public PerPayeRefreshEventsFixture SetupProcessDeclarations(long accountId, string empRef, decimal amount)
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

    public List<RefreshEmployerLevyDataCompletedEvent> GetPublishedEvents() =>
        _eventPublisher.Events.OfType<RefreshEmployerLevyDataCompletedEvent>().ToList();
}
