using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetCollectionPeriodPaymentsByAccountId;
using FundingSource = SFA.DAS.Provider.Events.Api.Types.FundingSource;
using TransactionType = SFA.DAS.Provider.Events.Api.Types.TransactionType;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetCollectionPeriodPaymentsByAccountIdTests;

public class WhenIGetCollectionPeriodPaymentsByAccountId
{
    private const long AccountId = 12345;
    private const string PeriodEndId = "2627-R03";

    private Mock<IPaymentRepository> _paymentRepository;
    private GetCollectionPeriodPaymentsByAccountIdQueryHandler _handler;
    private GetCollectionPeriodPaymentsByAccountIdQuery _query;

    [SetUp]
    public void Setup()
    {
        _paymentRepository = new Mock<IPaymentRepository>();
        _handler = new GetCollectionPeriodPaymentsByAccountIdQueryHandler(_paymentRepository.Object);
        _query = new GetCollectionPeriodPaymentsByAccountIdQuery(AccountId, 2026, 10);

        _paymentRepository
            .Setup(r => r.GetPeriodEnd(2026, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PeriodEnd { PeriodEndId = PeriodEndId, CalendarPeriodYear = 2026, CalendarPeriodMonth = 10 });
        SetupAccountPayments();
        SetupTransferOutPayments();
    }

    [Test]
    public async Task Then_No_Payments_Are_Returned_When_The_Period_End_Has_Not_Run()
    {
        // Arrange
        _paymentRepository
            .Setup(r => r.GetPeriodEnd(2026, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PeriodEnd)null);

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.PeriodEndCompleted.Should().BeFalse();
        result.Payments.Should().BeEmpty();
        _paymentRepository.Verify(r => r.GetAccountPaymentsForPeriodEnd(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Then_The_Period_End_Is_Completed_With_No_Payments_When_The_Account_Had_None()
    {
        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.PeriodEndCompleted.Should().BeTrue();
        result.Payments.Should().BeEmpty();
    }

    [Test]
    public async Task Then_The_Accounts_Payments_Are_Totalled_By_Funding_Source_And_Transaction_Type()
    {
        // Arrange
        SetupAccountPayments(
            CreatePayment(FundingSource.Levy, TransactionType.Learning, 100),
            CreatePayment(FundingSource.Levy, TransactionType.Learning, 150),
            CreatePayment(FundingSource.Levy, TransactionType.Completion, 300),
            CreatePayment(FundingSource.CoInvestedEmployer, TransactionType.Learning, 5),
            CreatePayment(FundingSource.CoInvestedEmployer, TransactionType.Learning, 10),
            CreatePayment(FundingSource.CoInvestedSfa, TransactionType.Learning, 95),
            CreatePayment(FundingSource.CoInvestedSfa, TransactionType.Learning, 190));

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.PeriodEndCompleted.Should().BeTrue();
        result.Payments.Should().BeEquivalentTo(new[]
        {
            new CollectionPeriodPaymentTotal(FundingSource.Levy, TransactionType.Learning, 250),
            new CollectionPeriodPaymentTotal(FundingSource.Levy, TransactionType.Completion, 300),
            new CollectionPeriodPaymentTotal(FundingSource.CoInvestedEmployer, TransactionType.Learning, 15),
            new CollectionPeriodPaymentTotal(FundingSource.CoInvestedSfa, TransactionType.Learning, 285),
        });
    }

    [TestCase(FundingSource.FullyFundedSfa)]
    [TestCase(FundingSource.LevyTransfer)]
    public async Task Then_The_Account_Does_Not_See_Its_Own_Payments_From_Other_Funding_Sources(FundingSource fundingSource)
    {
        // Arrange
        SetupAccountPayments(CreatePayment(fundingSource, TransactionType.Learning, 100));

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Payments.Should().BeEmpty();
    }

    [Test]
    public async Task Then_The_Sender_Sees_Only_The_Levy_Transfer_Payments_For_Its_Transfers()
    {
        // Arrange
        SetupTransferOutPayments(
            CreatePayment(FundingSource.LevyTransfer, TransactionType.Learning, 200),
            CreatePayment(FundingSource.LevyTransfer, TransactionType.Completion, 400),
            // The receiver's own payments for the same apprenticeship belong to the receiver
            CreatePayment(FundingSource.Levy, TransactionType.Learning, 50),
            CreatePayment(FundingSource.CoInvestedEmployer, TransactionType.Learning, 5));

        // Act
        var result = await _handler.Handle(_query, CancellationToken.None);

        // Assert
        result.Payments.Should().BeEquivalentTo(new[]
        {
            new CollectionPeriodPaymentTotal(FundingSource.LevyTransfer, TransactionType.Learning, 200),
            new CollectionPeriodPaymentTotal(FundingSource.LevyTransfer, TransactionType.Completion, 400),
        });
        _paymentRepository.Verify(r => r.GetTransferOutPaymentsForPeriodEnd(AccountId, PeriodEndId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private void SetupAccountPayments(params Payment[] payments) =>
        _paymentRepository
            .Setup(r => r.GetAccountPaymentsForPeriodEnd(AccountId, PeriodEndId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments.ToList());

    private void SetupTransferOutPayments(params Payment[] payments) =>
        _paymentRepository
            .Setup(r => r.GetTransferOutPaymentsForPeriodEnd(AccountId, PeriodEndId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments.ToList());

    private static Payment CreatePayment(FundingSource fundingSource, TransactionType transactionType, decimal amount) => new()
    {
        Id = Guid.NewGuid(),
        PeriodEnd = PeriodEndId,
        FundingSource = fundingSource,
        TransactionType = transactionType,
        Amount = amount
    };
}
