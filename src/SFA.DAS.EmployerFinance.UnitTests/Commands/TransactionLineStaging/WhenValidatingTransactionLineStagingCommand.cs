using SFA.DAS.EmployerFinance.Commands.TransactionLineStaging;
using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.TransactionLineStaging;

public class WhenValidatingTransactionLineStagingCommand
{
    private TransactionLineStagingCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new TransactionLineStagingCommandValidator();
    }

    [Test]
    public void Then_Negative_Payment_Amounts_Are_Valid()
    {
        var result = _validator.Validate(new TransactionLineStagingCommand
        {
            TransactionLines =
            [
                new TransactionLineStagingModel
                {
                    AccountId = 12345,
                    DateCreated = new DateTime(2025, 1, 2),
                    TransactionDate = new DateTime(2025, 1, 1),
                    TransactionType = 3,
                    Amount = -100,
                    SfaCoInvestmentAmount = -20,
                    EmployerCoInvestmentAmount = -10,
                    PeriodEnd = "2025-01",
                    Ukprn = 10000494
                }
            ]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_Mandatory_Fields_Are_Still_Validated()
    {
        var result = _validator.Validate(new TransactionLineStagingCommand
        {
            TransactionLines =
            [
                new TransactionLineStagingModel()
            ]
        });

        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Values.Should().Contain("AccountId is mandatory and must be > 0.");
        result.ValidationDictionary.Values.Should().Contain("TransactionDate is mandatory.");
    }

    [Test]
    public void Then_Transaction_Type_Must_Fit_In_A_TinyInt()
    {
        var result = _validator.Validate(new TransactionLineStagingCommand
        {
            TransactionLines =
            [
                new TransactionLineStagingModel
                {
                    AccountId = 12345,
                    DateCreated = new DateTime(2025, 1, 2),
                    TransactionDate = new DateTime(2025, 1, 1),
                    TransactionType = 256,
                    Amount = -100,
                    SfaCoInvestmentAmount = 0,
                    EmployerCoInvestmentAmount = 0,
                    PeriodEnd = "2025-01",
                    Ukprn = 10000494
                }
            ]
        });

        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Values.Should().Contain("TransactionType is not a valid value.");
    }
}
