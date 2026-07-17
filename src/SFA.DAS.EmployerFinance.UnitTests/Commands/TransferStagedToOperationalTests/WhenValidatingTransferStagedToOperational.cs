using SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.TransferStagedToOperationalTests;

public class WhenValidatingTransferStagedToOperational
{
    private TransferStagedToOperationalCommandValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new TransferStagedToOperationalCommandValidator();
    }

    [Test]
    public void Then_Is_Valid_When_AccountId_And_PeriodEnd_Are_Present()
    {
        var result = _validator.Validate(new TransferStagedToOperationalCommand
        {
            AccountId = 123,
            PeriodEnd = "2526-R01"
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void Then_Is_Invalid_When_AccountId_Is_Missing()
    {
        var result = _validator.Validate(new TransferStagedToOperationalCommand
        {
            AccountId = 0,
            PeriodEnd = "2526-R01"
        });

        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Values.Should().Contain("AccountId must be greater than 0");
    }

    [Test]
    public void Then_Is_Invalid_When_PeriodEnd_Is_Missing()
    {
        var result = _validator.Validate(new TransferStagedToOperationalCommand
        {
            AccountId = 123,
            PeriodEnd = " "
        });

        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Values.Should().Contain("PeriodEnd is required");
    }
}
