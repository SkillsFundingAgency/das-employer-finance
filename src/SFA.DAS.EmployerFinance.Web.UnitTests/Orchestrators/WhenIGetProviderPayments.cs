using System.Linq.Expressions;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators;

internal class WhenIGetProviderPayments
{
    private const string HashedAccountId = "123ABC";
    private const long AccountId = 1234;
    private const long ExpectedUkPrn = 46789465;
    private readonly DateTime _fromDate = DateTime.Now.AddDays(-20);
    private readonly DateTime _toDate = DateTime.Now.AddDays(-20);
    private Mock<ICurrentDateTime> _currentTime;
    private Mock<IEncodingService> _encodingService;

    private Mock<IAccountApiClient> _accountApiClient;
    private Mock<IMediator> _mediator;
    private EmployerAccountTransactionsOrchestrator _orchestrator;
    private FindAccountProviderPaymentsResponse _response;

    [SetUp]
    public void Arrange()
    {
        _accountApiClient = new Mock<IAccountApiClient>();
        _mediator = new Mock<IMediator>();
        _currentTime = new Mock<ICurrentDateTime>();
        _encodingService = new Mock<IEncodingService>();

        _response = new FindAccountProviderPaymentsResponse
        {
            ProviderName = "Test Provider",
            TransactionDate = DateTime.Now,
            Total = 100,
            Transactions = new List<PaymentTransactionLine>
            {
                new PaymentTransactionLine {Amount = 100}
            }
        };

        _mediator.Setup(AssertExpressionValidation()).ReturnsAsync(_response);

        _encodingService.Setup(h => h.Decode(HashedAccountId,EncodingType.AccountId)).Returns(AccountId);

        _accountApiClient.Setup(s => s.GetAccount(HashedAccountId))
            .Returns(Task.FromResult(new EAS.Account.Api.Types.AccountDetailViewModel
            {
                ApprenticeshipEmployerType = "Levy"
            }));

        _orchestrator =
            new EmployerAccountTransactionsOrchestrator(_accountApiClient.Object, _mediator.Object,
                _currentTime.Object, Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>(),
                Mock.Of<IEncodingService>(),Mock.Of<IAuthenticationOrchestrator>(),Mock.Of<IUserAccountService>());
    }

    private Expression<Func<IMediator, Task<FindAccountProviderPaymentsResponse>>> AssertExpressionValidation()
    {
        return x => x.Send(It.Is<FindAccountProviderPaymentsQuery>(c => 
            c.FromDate.Equals(_fromDate)
            && c.ToDate.Equals(_toDate)
            && c.HashedAccountId.Equals(
                HashedAccountId)
            && c.UkPrn.Equals(ExpectedUkPrn)), CancellationToken.None);
    }


    [Test]
    public async Task ThenIShouldGetTotalsByCourseForSFACoInvestmentPayments()
    {
        //Arrange
        var payment1 = new PaymentTransactionLine
        {
            CourseName = "Test Course",
            SfaCoInvestmentAmount = 100,
            TransactionType = TransactionItemType.Payment
        };
        var payment2 = new PaymentTransactionLine
        {
            CourseName = "Test Course",
            SfaCoInvestmentAmount = 50,
            TransactionType = TransactionItemType.Payment
        };
        var expectedTotal = payment1.SfaCoInvestmentAmount + payment2.SfaCoInvestmentAmount;

        _response = new FindAccountProviderPaymentsResponse
        {
            ProviderName = "Test Provider",
            TransactionDate = DateTime.Now,
            Total = expectedTotal,
            Transactions = new List<PaymentTransactionLine> {payment1, payment2}
        };
        _mediator.Setup(AssertExpressionValidation()).ReturnsAsync(_response);

        //Act
        var result = await _orchestrator.GetProviderPaymentSummary(HashedAccountId, ExpectedUkPrn, _fromDate,
            _toDate);

        //Assert
        result.Data.CoursePayments.First().SFACoInvestmentAmount.Should().Be(expectedTotal);
    }

    [Test]
    public async Task ThenIShouldGetTotalsByCourseForEmployerCoInvestmentPayments()
    {
        //Arrange
        var payment1 = new PaymentTransactionLine
        {
            CourseName = "Test Course",
            EmployerCoInvestmentAmount = 100,
            TransactionType = TransactionItemType.Payment
        };
        var payment2 = new PaymentTransactionLine
        {
            CourseName = "Test Course",
            EmployerCoInvestmentAmount = 50,
            TransactionType = TransactionItemType.Payment
        };
        var expectedTotal = payment1.EmployerCoInvestmentAmount + payment2.EmployerCoInvestmentAmount;

        _response = new FindAccountProviderPaymentsResponse
        {
            ProviderName = "Test Provider",
            TransactionDate = DateTime.Now,
            Total = expectedTotal,
            Transactions = new List<PaymentTransactionLine> {payment1, payment2}
        };
        _mediator.Setup(AssertExpressionValidation()).ReturnsAsync(_response);

        //Act
        var result = await _orchestrator.GetProviderPaymentSummary(HashedAccountId, ExpectedUkPrn, _fromDate,
            _toDate);

        //Assert
        result.Data.CoursePayments.First().EmployerCoInvestmentAmount.Should().Be(expectedTotal);
    }


    [Test]
    public async Task ThenIShouldGetTotalsByCourseForPaymentOverallTotal()
    {
        //Arrange
        var payment = new PaymentTransactionLine
        {
            CourseName = "Test Course",
            LineAmount = 100,
            SfaCoInvestmentAmount = 90,
            EmployerCoInvestmentAmount = 10,
            TransactionType = TransactionItemType.Payment
        };

        var expectedTotal = payment.LineAmount + payment.SfaCoInvestmentAmount + payment.EmployerCoInvestmentAmount;

        _response = new FindAccountProviderPaymentsResponse
        {
            ProviderName = "Test Provider",
            TransactionDate = DateTime.Now,
            Total = expectedTotal,
            Transactions = new List<PaymentTransactionLine> {payment}
        };
        _mediator.Setup(AssertExpressionValidation()).ReturnsAsync(_response);

        //Act
        var result = await _orchestrator.GetProviderPaymentSummary(HashedAccountId, ExpectedUkPrn, _fromDate,
            _toDate);

        //Assert
        result.Data.CoursePayments.First().TotalAmount.Should().Be(expectedTotal);
    }


    [Test]
    public async Task ThenIShouldGetTotalsForAllCourses()
    {
        //Arrange
        var payment1 = new PaymentTransactionLine
        {
            CourseName = "Test Course",
            LineAmount = 100,
            SfaCoInvestmentAmount = 90,
            EmployerCoInvestmentAmount = 10,
            TransactionType = TransactionItemType.Payment
        };

        var payment2 = new PaymentTransactionLine
        {
            CourseName = "Test Course 2",
            LineAmount = 100,
            SfaCoInvestmentAmount = 90,
            EmployerCoInvestmentAmount = 10,
            TransactionType = TransactionItemType.Payment
        };

        var expectedLevyPaymentsTotal = payment1.LineAmount + payment2.LineAmount;
        var expectedSFACoInvestmentTotal = payment1.SfaCoInvestmentAmount + payment2.SfaCoInvestmentAmount;
        var expectedEmployerCoInvestmentTotal =
            payment1.EmployerCoInvestmentAmount + payment2.EmployerCoInvestmentAmount;
        var expectedPaymentsTotal = payment1.LineAmount +
                                    payment1.SfaCoInvestmentAmount +
                                    payment1.EmployerCoInvestmentAmount +
                                    payment2.LineAmount +
                                    payment2.SfaCoInvestmentAmount +
                                    payment2.EmployerCoInvestmentAmount;

        _response = new FindAccountProviderPaymentsResponse
        {
            ProviderName = "Test Provider",
            TransactionDate = DateTime.Now,
            Total = expectedPaymentsTotal,
            Transactions = new List<PaymentTransactionLine> {payment1, payment2}
        };
        _mediator.Setup(AssertExpressionValidation()).ReturnsAsync(_response);

        //Act
        var result = await _orchestrator.GetProviderPaymentSummary(HashedAccountId, ExpectedUkPrn, _fromDate,
            _toDate);

        //Assert
        result.Data.LevyPaymentsTotal.Should().Be(expectedLevyPaymentsTotal);
        result.Data.SFACoInvestmentsTotal.Should().Be(expectedSFACoInvestmentTotal);
        result.Data.EmployerCoInvestmentsTotal.Should().Be(expectedEmployerCoInvestmentTotal);
        result.Data.PaymentsTotal.Should().Be(expectedPaymentsTotal);
    }
}