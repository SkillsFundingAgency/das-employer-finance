using AutoMapper;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.UnitTests.Mappings;

public class WhenIMapTransactionLineEntity
{
    private IMapper _mapper;

    [SetUp]
    public void Arrange()
    {
        var config = new MapperConfiguration(c => c.AddProfile<TransactionMappings>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Then_Payment_TransactionLineEntity_Maps_To_PaymentTransactionLine()
    {
        var entity = new TransactionLineEntity
        {
            Id = 10,
            AccountId = 12345,
            TransactionType = TransactionItemType.Payment,
            TransactionDate = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            DateCreated = new DateTime(2024, 1, 16, 12, 0, 0, DateTimeKind.Utc),
            Amount = -100.5m,
            UkPrn = 10001234,
            PeriodEnd = "2425-R06",
            SfaCoInvestmentAmount = -10m,
            EmployerCoInvestmentAmount = -5m
        };

        var result = _mapper.Map<PaymentTransactionLine>(entity);

        result.AccountId.Should().Be(entity.AccountId);
        result.TransactionType.Should().Be(TransactionItemType.Payment);
        result.TransactionDate.Should().Be(entity.TransactionDate);
        result.DateCreated.Should().Be(entity.DateCreated);
        result.Amount.Should().Be(entity.Amount);
        result.UkPrn.Should().Be(entity.UkPrn.Value);
        result.PeriodEnd.Should().Be(entity.PeriodEnd);
        result.SfaCoInvestmentAmount.Should().Be(entity.SfaCoInvestmentAmount);
        result.EmployerCoInvestmentAmount.Should().Be(entity.EmployerCoInvestmentAmount);
    }

    [Test]
    public void Then_Transfer_TransactionLineEntity_Maps_To_TransferTransactionLine()
    {
        var entity = new TransactionLineEntity
        {
            AccountId = 111,
            TransactionType = TransactionItemType.Transfer,
            TransactionDate = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            DateCreated = new DateTime(2024, 2, 1, 1, 0, 0, DateTimeKind.Utc),
            Amount = -250m,
            PeriodEnd = "2425-R07",
            TransferSenderAccountId = 111,
            TransferSenderAccountName = "Sender Ltd",
            TransferReceiverAccountId = 222,
            TransferReceiverAccountName = "Receiver Ltd"
        };

        var result = _mapper.Map<TransferTransactionLine>(entity);

        result.AccountId.Should().Be(entity.AccountId);
        result.TransactionType.Should().Be(TransactionItemType.Transfer);
        result.Amount.Should().Be(entity.Amount);
        result.PeriodEnd.Should().Be(entity.PeriodEnd);
        result.SenderAccountId.Should().Be(entity.TransferSenderAccountId.Value);
        result.SenderAccountName.Should().Be(entity.TransferSenderAccountName);
        result.ReceiverAccountId.Should().Be(entity.TransferReceiverAccountId.Value);
        result.ReceiverAccountName.Should().Be(entity.TransferReceiverAccountName);
    }

    [Test]
    public void Then_Levy_TransactionLineEntity_Maps_To_LevyDeclarationTransactionLine()
    {
        var entity = new TransactionLineEntity
        {
            AccountId = 55,
            TransactionType = TransactionItemType.Declaration,
            TransactionDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            DateCreated = new DateTime(2024, 3, 1, 1, 0, 0, DateTimeKind.Utc),
            Amount = 1000m,
            SubmissionId = 98765,
            EmpRef = "123/ABC",
            EnglishFraction = 0.85m
        };

        var result = _mapper.Map<LevyDeclarationTransactionLine>(entity);

        result.AccountId.Should().Be(entity.AccountId);
        result.TransactionType.Should().Be(TransactionItemType.Declaration);
        result.Amount.Should().Be(entity.Amount);
        result.SubmissionId.Should().Be(entity.SubmissionId.Value);
        result.EmpRef.Should().Be(entity.EmpRef);
        result.EnglishFraction.Should().Be(entity.EnglishFraction.Value);
    }

    [Test]
    public void Then_Unknown_TransactionLineEntity_Maps_To_TransactionLine()
    {
        var entity = new TransactionLineEntity
        {
            AccountId = 77,
            TransactionType = TransactionItemType.Unknown,
            TransactionDate = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc),
            DateCreated = new DateTime(2024, 4, 1, 1, 0, 0, DateTimeKind.Utc),
            Amount = 1m
        };

        var result = _mapper.Map<TransactionLine>(entity);

        result.AccountId.Should().Be(entity.AccountId);
        result.TransactionType.Should().Be(TransactionItemType.Unknown);
        result.Amount.Should().Be(entity.Amount);
    }

    [Test]
    public void Then_ExpiredFund_TransactionLineEntity_Maps_To_ExpiredFundTransactionLine()
    {
        var entity = new TransactionLineEntity
        {
            AccountId = 88,
            TransactionType = TransactionItemType.ExpiredFund,
            TransactionDate = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            DateCreated = new DateTime(2024, 5, 1, 1, 0, 0, DateTimeKind.Utc),
            Amount = -50m
        };

        var result = _mapper.Map<ExpiredFundTransactionLine>(entity);

        result.AccountId.Should().Be(entity.AccountId);
        result.TransactionType.Should().Be(TransactionItemType.ExpiredFund);
        result.Amount.Should().Be(entity.Amount);
    }
}
