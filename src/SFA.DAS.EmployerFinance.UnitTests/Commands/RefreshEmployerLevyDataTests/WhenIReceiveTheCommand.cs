using System.ComponentModel.DataAnnotations;

using SFA.DAS.EAS.Account.Api.Types.Events.Levy;
using SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.UnitTests.ObjectMothers;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Testing.Services;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshEmployerLevyDataTests;

public class WhenIReceiveTheCommand
{
    private RefreshEmployerLevyDataCommandHandler _refreshEmployerLevyDataCommandHandler;
    private Mock<IValidator<RefreshEmployerLevyDataCommand>> _validator;
    private Mock<IDasLevyRepository> _levyRepository;
    private Mock<IMediator> _mediator;
    private Mock<IHmrcDateService> _hmrcDateService;
    private Mock<IEncodingService> _encodingService;
    private ILevyImportCleanerStrategy _levyImportCleanerStrategy;
    private Mock<ILogger<LevyImportCleanerStrategy>> _logger;
    private Mock<ICurrentDateTime> _currentDateTime;
    private TestableEventPublisher _eventPublisher;
    private const string ExpectedEmpRef = "123456";
    private const long ExpectedAccountId = 44321;

    [SetUp]
    public void Arrange()
    {
        _levyRepository = new Mock<IDasLevyRepository>();
        _levyRepository.Setup(x => x.GetLastSubmissionForScheme(ExpectedEmpRef)).ReturnsAsync(new DasDeclaration { LevyDueYtd = 1000m, LevyAllowanceForFullYear = 1200m });

        _validator = new Mock<IValidator<RefreshEmployerLevyDataCommand>>();
        _validator.Setup(x => x.Validate(It.IsAny<RefreshEmployerLevyDataCommand>())).Returns(new ValidationResult());

        _mediator = new Mock<IMediator>();

        _hmrcDateService = new Mock<IHmrcDateService>();
        _hmrcDateService.Setup(x => x.IsSubmissionForFuturePeriod(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<DateTime>())).Returns(false);

        _currentDateTime = new Mock<ICurrentDateTime>();
        _currentDateTime.Setup(cdt => cdt.Now).Returns(() => DateTime.UtcNow);

        _encodingService = new Mock<IEncodingService>();
        _logger = new Mock<ILogger<LevyImportCleanerStrategy>>();
        _eventPublisher = new TestableEventPublisher();
        _levyImportCleanerStrategy = new LevyImportCleanerStrategy(_levyRepository.Object, _hmrcDateService.Object, _logger.Object, _currentDateTime.Object);

        _refreshEmployerLevyDataCommandHandler = new RefreshEmployerLevyDataCommandHandler(_validator.Object, _levyRepository.Object, _levyImportCleanerStrategy, _eventPublisher, Mock.Of<ILogger<RefreshEmployerLevyDataCommandHandler>>());
    }

    [Test]
    public async Task ThenTheValidatorIsCalled()
    {
        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(RefreshEmployerLevyDataCommandObjectMother.Create(ExpectedEmpRef), CancellationToken.None);

        //Assert
        _validator.Verify(x => x.Validate(It.IsAny<RefreshEmployerLevyDataCommand>()));
    }

    [Test]
    public void ThenAInvalidRequestExceptionIsThrownIfTheMessageIsNotValid()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<RefreshEmployerLevyDataCommand>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act
        Assert.ThrowsAsync<ValidationException>(async () => await _refreshEmployerLevyDataCommandHandler.Handle(new RefreshEmployerLevyDataCommand(), CancellationToken.None));
    }

    [Test]
    public async Task ThenTheExistingDeclarationIdsAreCollected()
    {
        //Arrange
        var refreshEmployerLevyDataCommand = RefreshEmployerLevyDataCommandObjectMother.Create(ExpectedEmpRef);

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(refreshEmployerLevyDataCommand, CancellationToken.None);

        //Assert
        _levyRepository.Verify(x => x.GetEmployerDeclarationSubmissionIds(ExpectedEmpRef), Times.Once());
    }

    [Test]
    public async Task ThenTheLevyRepositoryIsUpdatedIfTheDeclarationDoesNotExist()
    {
        //Arrange
        _levyRepository.Setup(x => x.GetEmployerDeclarationSubmissionIds(ExpectedEmpRef)).ReturnsAsync(new List<long> { 2 });

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(RefreshEmployerLevyDataCommandObjectMother.Create(ExpectedEmpRef, ExpectedAccountId), CancellationToken.None);

        //Assert
        _levyRepository.Verify(x => x.CreateEmployerDeclarations(It.IsAny<IEnumerable<DasDeclaration>>(), ExpectedEmpRef, ExpectedAccountId));
    }

    [Test]
    public async Task ThenIfThereAreDeclarationsToProcessTheLevyAddedToAccountEventAndAccountLevyStatusEventIsPublished()
    {
        //Arrange
        var data = RefreshEmployerLevyDataCommandObjectMother.Create(ExpectedEmpRef, ExpectedAccountId);
        _levyRepository.Setup(m => m.ProcessDeclarations(ExpectedAccountId, It.IsAny<string>())).Returns(Task.FromResult(decimal.One));

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);

        //Assert
        _levyRepository.Verify(x => x.ProcessDeclarations(ExpectedAccountId, ExpectedEmpRef), Times.Once);

        (_eventPublisher.Events.OfType<LevyAddedToAccount>().Any(e =>
            e.AccountId.Equals(ExpectedAccountId)
            && e.Amount.Equals(decimal.One))).Should().BeTrue();

        (_eventPublisher.Events.OfType<RefreshEmployerLevyDataCompletedEvent>().Any(e =>
            e.AccountId.Equals(ExpectedAccountId) &&
            e.LevyImported.Equals(true) &&
            e.LevyTransactionValue.Equals(decimal.One))).Should().BeTrue();
    }

    [Test]
    public async Task ThenIfThereAreNoNewDeclarationsThenTheProcessDeclarationEventIsNotPublished()
    {
        //Arrange
        _levyRepository.Setup(x => x.GetEmployerDeclarationSubmissionIds(ExpectedEmpRef)).ReturnsAsync(new List<long> { 1, 2, 3, 4 });
        var data = RefreshEmployerLevyDataCommandObjectMother.Create(ExpectedEmpRef, ExpectedAccountId);

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);

        //Assert
        _levyRepository.Verify(x => x.ProcessDeclarations(ExpectedAccountId, ExpectedEmpRef), Times.Never);

        _eventPublisher.Events.OfType<LevyAddedToAccount>().Any(e =>
            e.AccountId.Equals(ExpectedAccountId)
            && e.Amount.Equals(decimal.One)).Should().BeFalse();

        _eventPublisher.Events.OfType<RefreshEmployerLevyDataCompletedEvent>().Any(e =>
            e.AccountId.Equals(ExpectedAccountId) &&
            e.LevyImported.Equals(false) &&
            e.LevyTransactionValue.Equals(decimal.Zero)).Should().BeTrue();
    }

    [Test]
    public async Task ThenIfTheSubmissionIsAnEndOfYearAdjustmentThePeriod12ValueWillBeTakenFromHmrcIfItExists()
    {
        const decimal period12Value = 5;
        var period12SubmissionDate = new DateTime(2017, 04, 19);
        const decimal yearEndAdjustment = 20;
        var yearEndAdjustmentSubmissionDate = new DateTime(2017, 05, 01);

        // the sign on the end of year adjustment value stored on the levy declaration table is inverted (i.e. it will be -15 here, not 15).
        const decimal endOfYearAdjustmentAmount = -15;

        List<DasDeclaration> savedDeclarations = null;

        //Arrange
        _hmrcDateService.Setup(x => x.IsSubmissionEndOfYearAdjustment("16-17", 12, yearEndAdjustmentSubmissionDate)).Returns(true);
        _hmrcDateService.Setup(x => x.IsDateOntimeForPayrollPeriod("16-17", 12, period12SubmissionDate)).Returns(true);

        _levyRepository
            .Setup(x => x.CreateEmployerDeclarations(It.IsAny<IEnumerable<DasDeclaration>>(), ExpectedEmpRef, ExpectedAccountId))
            .Callback<IEnumerable<DasDeclaration>, string, long>((declarations, empref, accountId) => savedDeclarations = declarations.ToList())
            .Returns(Task.CompletedTask);

        // this creates a period 12 declaration and a year-end adjustment. The GetSubmissionByEmprefPayrollYearAndMonth() method should not be called
        var data = RefreshEmployerLevyDataCommandObjectMother.CreateEndOfYearAdjustmentToPeriod12DeclarationOnHmrcFeed(
            ExpectedEmpRef,
            ExpectedAccountId,
            period12Value,
            period12SubmissionDate,
            yearEndAdjustment,
            yearEndAdjustmentSubmissionDate);

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);

        //Assert
        savedDeclarations.Count.Should().Be(data.EmployerLevyData.Sum(eld => eld.Declarations.Declarations.Count), "Incorrect number of declarations saved");
        savedDeclarations.Any(ld => ld.EndOfYearAdjustment && ld.EndOfYearAdjustmentAmount == endOfYearAdjustmentAmount).Should().BeTrue("Year end adjustment not saved with expected end of year adjustment value");
    }

    [Test]
    public async Task ThenIfTheSubmissionIsForATaxMonthInTheFutureItWillNotBeProcessed()
    {
        //Arrange
        var data = RefreshEmployerLevyDataCommandObjectMother.CreateLevyDataWithFutureSubmissions(ExpectedEmpRef, DateTime.UtcNow, ExpectedAccountId);
        var declaration = data.EmployerLevyData.Last().Declarations.Declarations.Last();
        _hmrcDateService.Setup(x => x.IsSubmissionForFuturePeriod(declaration.PayrollYear, declaration.PayrollMonth.Value, It.IsAny<DateTime>())).Returns(true);

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);

        //Assert
        _levyRepository.Verify(x => x.CreateEmployerDeclarations(It.Is<IEnumerable<DasDeclaration>>(c => c.Count() == 4), ExpectedEmpRef, ExpectedAccountId), Times.Once);
    }
    
    [Test]
    public async Task ThenIfThePayrollYearPreDatesTheLevyItIsNotProcessed()
    {
        //Arrange
        var data = RefreshEmployerLevyDataCommandObjectMother.CreateLevyDataWithFutureSubmissions(ExpectedEmpRef, DateTime.Now);
        _hmrcDateService.Setup(x => x.DoesSubmissionPreDateLevy(It.IsAny<string>())).Returns(true);

        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);

        //Assert
        _levyRepository.Verify(x => x.CreateEmployerDeclarations(It.IsAny<IEnumerable<DasDeclaration>>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
    }

    [Test]
    public void ThenShouldThrowErrorIfAdjustmentLevyYtdIsNullAndIsNotANonPayment()
    {
        //Arrange
        var latestDeclaration = new DasDeclaration { LevyDueYtd = 20 };

        _hmrcDateService.Setup(x => x.IsSubmissionEndOfYearAdjustment("16-17", 12, It.IsAny<DateTime>()))
            .Returns(true);

        _levyRepository.Setup(x => x.GetSubmissionByEmprefPayrollYearAndMonth(ExpectedEmpRef, "16-17", 8))
            .ReturnsAsync(latestDeclaration);

        var data = RefreshEmployerLevyDataCommandObjectMother.CreateEndOfYearAdjustment(ExpectedEmpRef, ExpectedAccountId);

        var newDeclaration = data.EmployerLevyData.First().Declarations.Declarations.First();
        newDeclaration.LevyDueYtd = null;
        newDeclaration.NoPaymentForPeriod = false;

        //Act
        var action = () => _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);

        action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void ThenShouldNotThrowErrorIfAdjustmentLevyYtdIsNullAndIsANonPayment()
    {
        //Arrange
        var latestDeclaration = new DasDeclaration { LevyDueYtd = 20 };
        _hmrcDateService.Setup(x => x.IsSubmissionEndOfYearAdjustment("16-17", 12, It.IsAny<DateTime>()))
            .Returns(true);
        _levyRepository.Setup(x => x.GetSubmissionByEmprefPayrollYearAndMonth(ExpectedEmpRef, "16-17", 8))
            .ReturnsAsync(latestDeclaration);
        var data = RefreshEmployerLevyDataCommandObjectMother.CreateEndOfYearAdjustment(ExpectedEmpRef,
            ExpectedAccountId);
        var newDeclaration = data.EmployerLevyData.First().Declarations.Declarations.First();
        newDeclaration.LevyDueYtd = null;
        newDeclaration.NoPaymentForPeriod = true;
        //Act
        var action = () => _refreshEmployerLevyDataCommandHandler.Handle(data, CancellationToken.None);
        action.Should().NotThrowAsync();
    }


    [Test]
    public async Task ThenARefreshEmployerLevyDataCompletedEventIsPublished()
    {
        //Act
        await _refreshEmployerLevyDataCommandHandler.Handle(RefreshEmployerLevyDataCommandObjectMother.Create(ExpectedEmpRef, ExpectedAccountId), CancellationToken.None);

        //Assert
        _eventPublisher.Events.OfType<RefreshEmployerLevyDataCompletedEvent>().Any(e =>
            e.AccountId.Equals(ExpectedAccountId)).Should().BeTrue();
    }
}