using FluentAssertions;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Validation;

public class WhenAddingValidationErrors
{
    [Test]
    public void ThenDoesNotThrowWhenTheSamePropertyIsAddedTwice()
    {
        var result = new ValidationResult();

        var act = () =>
        {
            result.AddError("Amount", "Amount must be greater than 0");
            result.AddError("Amount", "Amount must be greater than 0");
        };

        act.Should().NotThrow();
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().ContainKey("Amount");
    }

    [Test]
    public void ThenMergesMessagesForTheSameProperty()
    {
        var result = new ValidationResult();

        result.AddError("TransferId", "TransferId is required");
        result.AddError("TransferId", "TransferId must be greater than 0");

        result.ValidationDictionary["TransferId"].Should().Be("TransferId is required TransferId must be greater than 0");
    }
}
