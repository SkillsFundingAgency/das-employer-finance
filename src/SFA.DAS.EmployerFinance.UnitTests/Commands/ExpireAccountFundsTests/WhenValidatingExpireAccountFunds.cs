using SFA.DAS.EmployerFinance.Commands.ExpireAccountFunds;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.ExpireAccountFundsTests;

public class WhenValidatingExpireAccountFunds
{
    private ExpireAccountFundsCommandValidator _validator = null!;

    [SetUp]
    public void Arrange()
    {
        _validator = new ExpireAccountFundsCommandValidator();
    }

    [Test]
    public void Then_A_Complete_Command_Is_Valid()
    {
        var result = _validator.Validate(new ExpireAccountFundsCommand
        {
            AccountId = 123,
            CorrelationId = "corr-123"
        });

        result.IsValid().Should().BeTrue();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Then_AccountId_Must_Be_Greater_Than_Zero(long accountId)
    {
        var result = _validator.Validate(new ExpireAccountFundsCommand
        {
            AccountId = accountId,
            CorrelationId = "corr-123"
        });

        result.ValidationDictionary.Should().Contain(
            nameof(ExpireAccountFundsCommand.AccountId),
            "AccountId must be greater than 0.");
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Then_CorrelationId_Is_Required(string correlationId)
    {
        var result = _validator.Validate(new ExpireAccountFundsCommand
        {
            AccountId = 123,
            CorrelationId = correlationId
        });

        result.ValidationDictionary.Should().Contain(
            nameof(ExpireAccountFundsCommand.CorrelationId),
            "CorrelationId is required.");
    }

    [Test]
    public void Then_CorrelationId_Must_Fit_The_Persisted_Request_Identifier()
    {
        var result = _validator.Validate(new ExpireAccountFundsCommand
        {
            AccountId = 123,
            CorrelationId = new string('c', 101)
        });

        result.ValidationDictionary.Should().Contain(
            nameof(ExpireAccountFundsCommand.CorrelationId),
            "CorrelationId must be 100 characters or fewer.");
    }
}
