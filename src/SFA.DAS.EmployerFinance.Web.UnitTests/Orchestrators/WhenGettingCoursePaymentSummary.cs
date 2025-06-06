﻿using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;
using ApprenticeshipEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators;

[Parallelizable]
public class WhenGettingCoursePaymentSummary
{
    private readonly IFixture _fixture = new Fixture();
    private EmployerAccountTransactionsOrchestrator _sut;
    private Mock<IAccountApiClient> _accountApiMock;
    private Mock<IMediator> _mediatorMock;
    private Mock<ICurrentDateTime> _currentTimeMock;

    [SetUp]
    public void SetUp()
    {
        _mediatorMock = new Mock<IMediator>();
        _accountApiMock = new Mock<IAccountApiClient>();
        _currentTimeMock = new Mock<ICurrentDateTime>();

        _accountApiMock.Setup(s => s.GetAccount(It.IsAny<string>()))
            .Returns(Task.FromResult(new EAS.Account.Api.Types.AccountDetailViewModel
            {
                ApprenticeshipEmployerType = "NonLevy"
            }));

        SetupGetCoursePaymentsResponse(2019, 9);

        _sut = new EmployerAccountTransactionsOrchestrator(
            _accountApiMock.Object, 
            _mediatorMock.Object, 
            _currentTimeMock.Object,
            Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>(), Mock.Of<IEncodingService>(), Mock.Of<IAuthenticationOrchestrator>(),Mock.Of<IGovAuthEmployerAccountService>());
    }

    [Test]
    [TestCase(1, Description = "Course payment summary for single apprentice")]
    [TestCase(9, Description = "Course payment summary for multiple apprentice")]
    public async Task ThenSummariesForEachApprenticeShouldBeCreated(int numberOfApprentices)
    {
        // Arrange
        var coursePayments = CreateCoursePayments(numberOfApprentices, 1);
        SetupGetCoursePaymentsResponse(2019, 9, coursePayments);

        // Act
        var response = await _sut.GetCoursePaymentSummary("abc123", 888888, "A course", 4, null, new DateTime(2019, 9, 1), new DateTime(2019, 9, 30));

        // Assert
        response.Data.ApprenticePayments.Count.Should().Be(numberOfApprentices);
    }

    [Test]
    public async Task ThenNonLevyEmployerShouldNotSeeNonCoInvestmentPaymentColumn_IfThereIsNoValue()
    {
        // Arrange
        _accountApiMock.Setup(s => s.GetAccount(It.IsAny<string>()))
            .Returns(Task.FromResult(new EAS.Account.Api.Types.AccountDetailViewModel
            {
                ApprenticeshipEmployerType = "NonLevy"
            }));

        var coursePayments = CreateCoursePayments(1, 1, 0, 900, 100);

        foreach (var coursePayment in coursePayments)
        {
            coursePayment.LineAmount = 0;
            coursePayment.EmployerCoInvestmentAmount = 100;
            coursePayment.SfaCoInvestmentAmount = 900;
        }

        SetupGetCoursePaymentsResponse(2019, 9, coursePayments);

        // Act
        var response = await _sut.GetCoursePaymentSummary("abc123", 888888, "A course", 4, null, new DateTime(2019, 9, 1), new DateTime(2019, 9, 30));

        // Assert
        response.Data.ShowNonCoInvesmentPaymentsTotal.Should().BeFalse();
    }

    [Test]
    public async Task ThenLevyEmployerShouldSeeNonCoInvestmentPaymentColumn_IfThereIsNoValue()
    {
        // Arrange
        _accountApiMock.Setup(s => s.GetAccount(It.IsAny<string>()))
            .Returns(Task.FromResult(new EAS.Account.Api.Types.AccountDetailViewModel
            {
                ApprenticeshipEmployerType = "Levy"
            }));

        var coursePayments = CreateCoursePayments(1, 1, 0, 900, 100);

        foreach (var coursePayment in coursePayments)
        {
            coursePayment.LineAmount = 0;
            coursePayment.EmployerCoInvestmentAmount = 100;
            coursePayment.SfaCoInvestmentAmount = 900;
        }

        SetupGetCoursePaymentsResponse(2019, 9, coursePayments);

        // Act
        var response = await _sut.GetCoursePaymentSummary("abc123", 888888, "A course", 4, null, new DateTime(2019, 9, 1), new DateTime(2019, 9, 30));

        // Assert
        response.Data.ShowNonCoInvesmentPaymentsTotal.Should().BeTrue();
    }

    [Test]
    [TestCase(ApprenticeshipEmployerType.Levy)]
    [TestCase(ApprenticeshipEmployerType.NonLevy)]
    public async Task ThenUserShouldSeeNonCoInvestmentPaymentColumn_IfThereIsValue(ApprenticeshipEmployerType apprenticeshipEmployerType)
    {
        // Arrange
        _accountApiMock.Setup(s => s.GetAccount(It.IsAny<string>()))
            .Returns(Task.FromResult(new EAS.Account.Api.Types.AccountDetailViewModel
            {
                ApprenticeshipEmployerType = apprenticeshipEmployerType.ToString()
            }));

        var coursePayments = CreateCoursePayments(1, 1, 1000, 0, 0);

        SetupGetCoursePaymentsResponse(2019, 9, coursePayments);

        // Act
        var response = await _sut.GetCoursePaymentSummary("abc123", 888888, "A course", 4, null, new DateTime(2019, 9, 1), new DateTime(2019, 9, 30));

        // Assert
        response.Data.ShowNonCoInvesmentPaymentsTotal.Should().BeTrue();
    }

    private IEnumerable<PaymentTransactionLine> CreateCoursePayments(
        int noOfApprentices, 
        int noOfPaymentsForApprentice, 
        decimal lineAmount = 100,
        decimal sfaCoInvestment = 100,
        decimal employerCoInvestment = 100)
    {
        var payments = new List<PaymentTransactionLine>();

        for (int i = 1; i <= noOfApprentices; i++)
        {
            payments.AddRange(_fixture
                .Build<PaymentTransactionLine>()
                .Without(ptl => ptl.SubTransactions)
                .With(ptl => ptl.TransactionType, TransactionItemType.Payment)
                .With(ptl => ptl.ApprenticeName, $"Apprentice-{0}")
                .With(ptl => ptl.ApprenticeNINumber, $"ApprenticeNI-{0}")
                .With(ptl => ptl.ApprenticeULN, i)
                .With(ptl => ptl.LineAmount, lineAmount)
                .With(ptl => ptl.SfaCoInvestmentAmount, sfaCoInvestment)
                .With(ptl => ptl.EmployerCoInvestmentAmount, employerCoInvestment)
                .CreateMany(noOfPaymentsForApprentice));
        }

        return payments;
    }

    private void SetupGetCoursePaymentsResponse(int year, int month)
    {
        SetupGetCoursePaymentsResponse(year, month, Array.Empty<PaymentTransactionLine>());
    }

    private void SetupGetCoursePaymentsResponse(int year, int month, IEnumerable<PaymentTransactionLine> payments)
    {
        _mediatorMock.Setup(x => x.Send(It.IsAny<FindAccountCoursePaymentsQuery>(), CancellationToken.None))
            .ReturnsAsync(new FindAccountCoursePaymentsResponse
            {
                Transactions = payments.ToList()
            });
    }
}