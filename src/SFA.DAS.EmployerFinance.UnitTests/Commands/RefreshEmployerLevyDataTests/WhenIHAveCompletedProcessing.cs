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
public class RefreshEmployerLevyDataCommandHandlerTests
{
    [Test]
    public async Task WhenIHaveCompletedProcessing_AndHaveNewLevy()
    {
        const long accountId = 999;
        const string empRef = "ABC/12345";

        var fixture = new RefreshEmployerLevyDataCommandHandlerTestsFixture();

        fixture.SetAccountId(accountId);
        fixture.SetIsSubmissionForFuturePeriod(false);
        fixture.SetLastSubmissionForScheme(empRef, new DasDeclaration { LevyDueYtd = 1000m, LevyAllowanceForFullYear = 1200m });
        fixture.SetEmployerLevyData(new List<EmployerLevyData>
        {
            new()
            {
                EmpRef = empRef,
                Declarations = new DasDeclarations
                {
                    Declarations = new List<DasDeclaration>
                    {
                        new()
                        {
                            LevyDueYtd = 2000,
                            PayrollYear = "2018",
                            PayrollMonth = 6
                        }
                    }
                }
            }
        });

        await fixture.Handle();

        fixture.VerifyRefreshEmployerLevyDataCompletedEventIsPublished(true);
    }
}

public class RefreshEmployerLevyDataCommandHandlerTestsFixture 
{
    private readonly TestableEventPublisher _eventPublisher;
    private readonly Mock<IDasLevyRepository> _dasLevyRepository;
    private readonly Mock<IHmrcDateService> _hmrcDateService;
    private readonly RefreshEmployerLevyDataCommandHandler _handler;
    private readonly Mock<ICurrentDateTime> _currentDateTime;
    private long _accountId = 999;
    private ICollection<EmployerLevyData> _employerLevyData;

    public RefreshEmployerLevyDataCommandHandlerTestsFixture()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        var logger = new Mock<ILogger<LevyImportCleanerStrategy>>();
        _eventPublisher = new TestableEventPublisher();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(cdt => cdt.Now).Returns(() => DateTime.UtcNow);
        var validator = new Mock<IValidator<RefreshEmployerLevyDataCommand>>();
        validator.Setup(x => x.Validate(It.IsAny<RefreshEmployerLevyDataCommand>())).Returns(new ValidationResult());

        _hmrcDateService = new Mock<IHmrcDateService>();

        var levyImportCleanerStrategy = new LevyImportCleanerStrategy(_dasLevyRepository.Object, _hmrcDateService.Object, logger.Object, _currentDateTime.Object);

        _handler = new RefreshEmployerLevyDataCommandHandler(validator.Object, _dasLevyRepository.Object, levyImportCleanerStrategy, _eventPublisher, Mock.Of<ILogger<RefreshEmployerLevyDataCommandHandler>>());
    }

    public RefreshEmployerLevyDataCommandHandlerTestsFixture SetAccountId(long accountId)
    {
        _accountId = accountId;

        return this;
    }

    public RefreshEmployerLevyDataCommandHandlerTestsFixture SetIsSubmissionForFuturePeriod(bool result)
    {
        _hmrcDateService.Setup(x => x.IsSubmissionForFuturePeriod(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<DateTime>())).Returns(result);

        return this;
    }

    public RefreshEmployerLevyDataCommandHandlerTestsFixture SetLastSubmissionForScheme(string empRef, DasDeclaration lastDeclaration)
    {
        _dasLevyRepository.Setup(x => x.GetLastSubmissionForScheme(empRef)).ReturnsAsync(lastDeclaration);

        return this;
    }

    public RefreshEmployerLevyDataCommandHandlerTestsFixture SetEmployerLevyData(ICollection<EmployerLevyData> employerLevyData)
    {
        _employerLevyData = employerLevyData;

        return this;
    }

    public Task Handle()
    {
        return _handler.Handle(new RefreshEmployerLevyDataCommand()
        {
            AccountId = _accountId,
            EmployerLevyData = _employerLevyData
        }, CancellationToken.None);
    }

    public void VerifyRefreshEmployerLevyDataCompletedEventIsPublished(bool expectedLevyImportedValue)
    {
        (_eventPublisher.Events.OfType<RefreshEmployerLevyDataCompletedEvent>().Any(e =>
            e.AccountId.Equals(_accountId)
            && e.LevyImported.Equals(expectedLevyImportedValue))).Should().BeTrue();
    }
}