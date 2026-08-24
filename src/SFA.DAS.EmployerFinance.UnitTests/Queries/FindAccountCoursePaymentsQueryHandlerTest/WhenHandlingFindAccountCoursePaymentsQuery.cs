using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using System.ComponentModel.DataAnnotations;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.FindAccountCoursePaymentsQueryHandlerTest;

[TestFixture]
internal class WhenHandlingFindAccountCoursePaymentsQuery : QueryBaseTest<FindAccountCoursePaymentsQueryHandler, FindAccountCoursePaymentsQuery, FindAccountCoursePaymentsResponse>
{
    private const string ProviderName = "Test Provider";
    private const string CourseName = "Test Course";
    private const int CourseLevel = 3;
    private const int PathwayCode = 1;
    private const string PathwayName = "Test Pathway";

    private Mock<IDasLevyService> _dasLevyService;
    private Mock<IEncodingService> _encodingService;
    private DateTime _fromDate;
    private DateTime _toDate;
    private long _accountId;
    private long _ukprn;
    private string _hashedAccountId;

    public override FindAccountCoursePaymentsQuery Query { get; set; }
    public override FindAccountCoursePaymentsQueryHandler RequestHandler { get; set; }
    public override Mock<IValidator<FindAccountCoursePaymentsQuery>> RequestValidator { get; set; }

    [SetUp]
    public void Arrange()
    {
        SetUp();

        _fromDate = DateTime.Now.AddDays(-10);
        _toDate = DateTime.Now.AddDays(-2);
        _accountId = 1;
        _ukprn = 10;
        _hashedAccountId = "123ABC";

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId)).Returns(_accountId);

        _dasLevyService = new Mock<IDasLevyService>();
        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([
                new PaymentTransactionLine
                {
                    ProviderName = ProviderName,
                    CourseName = CourseName,
                    CourseLevel = CourseLevel,
                    PathwayName = PathwayName,
                    LearningType = Common.Domain.Types.LearningType.Apprenticeship
                }
            ]);

        Query = new FindAccountCoursePaymentsQuery
        {
            HashedAccountId = _hashedAccountId,
            UkPrn = _ukprn,
            CourseName = CourseName,
            CourseLevel = CourseLevel,
            PathwayCode = PathwayCode,
            FromDate = _fromDate,
            ToDate = _toDate
        };

        RequestHandler = new FindAccountCoursePaymentsQueryHandler(
            RequestValidator.Object,
            _dasLevyService.Object,
            _encodingService.Object);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        //Act
        await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        _encodingService.Verify(x => x.Decode(_hashedAccountId, EncodingType.AccountId), Times.Once);
        _dasLevyService.Verify(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
            (_accountId, _ukprn, CourseName, CourseLevel, PathwayCode, _fromDate, _toDate));
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.Should().NotBeNull();
        actual.Transactions.Should().NotBeEmpty();
    }

    [Test]
    public void ThenAValidationExceptionIsThrownIfTheValidationResultIsInvalid()
    {
        //Arrange
        RequestValidator.Setup(x => x.ValidateAsync(It.IsAny<FindAccountCoursePaymentsQuery>()))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = { { "Error", "Invalid" } } });

        //Act Assert
        Assert.ThrowsAsync<ValidationException>(async () => await RequestHandler.Handle(new FindAccountCoursePaymentsQuery(), CancellationToken.None));
    }

    [Test]
    public async Task ThenTheProviderNameShouldBeAddedToTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.ProviderName.Should().Be(ProviderName);
    }

    [Test]
    public async Task ThenTheCourseNameShouldBeAddedToTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.CourseName.Should().Be(CourseName);
    }

    [Test]
    public async Task ThenTheCourseLevelShouldBeAddedToTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.CourseLevel.Should().Be(CourseLevel);
    }

    [Test]
    public async Task ThenThePathwayNameShouldBeAddedToTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.PathwayName.Should().Be(PathwayName);
    }

    [Test]
    public async Task ThenTheLearningTypeShouldBeAddedToTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.LearningType.Should().Be(Common.Domain.Types.LearningType.Apprenticeship);
    }

    [Test]
    public async Task ThenTheTransactionDateShouldBeAddedToTheResponse()
    {
        //Arrange
        var transactionDate = DateTime.Now.AddDays(-2);
        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([
                new PaymentTransactionLine { TransactionDate = transactionDate }
            ]);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.TransactionDate.Should().Be(transactionDate);
    }

    [Test]
    public async Task ThenTheDateCreatedShouldBeAddedToTheResponse()
    {
        //Arrange
        var dateCreated = DateTime.Now.AddDays(-3);
        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([
                new PaymentTransactionLine { DateCreated = dateCreated }
            ]);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.DateCreated.Should().Be(dateCreated);
    }

    [Test]
    public async Task ThenTheTotalShouldBeTheSumOfAllTransactionLineAmounts()
    {
        //Arrange
        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([
                new PaymentTransactionLine { LineAmount = 100m },
                new PaymentTransactionLine { LineAmount = 250m }
            ]);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.Total.Should().Be(350m);
    }

    [Test]
    public async Task ThenTheCohortReferenceShouldBeEncodedWhenCohortIdIsPresent()
    {
        //Arrange
        const long cohortId = 999;
        const string encodedCohortReference = "ENCODED-COHORT";

        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([
                new PaymentTransactionLine { CohortId = cohortId }
            ]);

        _encodingService.Setup(x => x.Encode(cohortId, EncodingType.CohortReference)).Returns(encodedCohortReference);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.CohortReference.Should().Be(encodedCohortReference);
    }

    [Test]
    public async Task ThenTheCohortReferenceShouldBeNullWhenCohortIdIsNotPresent()
    {
        //Arrange
        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([
                new PaymentTransactionLine { CohortId = null }
            ]);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.CohortReference.Should().BeNull();
        _encodingService.Verify(x => x.Encode(It.IsAny<long>(), EncodingType.CohortReference), Times.Never);
    }

    [Test]
    public async Task ThenNullShouldBeReturnedIfNoTransactionsAreFound()
    {
        //Arrange
        _dasLevyService.Setup(x => x.GetAccountCoursePaymentsByDateRange<PaymentTransactionLine>
                (It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync([]);

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.Should().BeNull();
    }
}