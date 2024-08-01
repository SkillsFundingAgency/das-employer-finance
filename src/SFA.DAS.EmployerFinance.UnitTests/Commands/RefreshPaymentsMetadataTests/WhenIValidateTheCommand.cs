using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshPaymentsMetadataTests;

public class WhenIValidateTheCommand
{
    [Test, MoqAutoData]
    public void ThenTrueIsReturnedWhenAllFieldsAreValid(
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandValidator validator
        )
    {
        var actual = validator.Validate(command);

        actual.IsValid().Should().BeTrue();
    }
    
    [Test, MoqAutoData]
    public void ThenFalseIsReturnedWhenFieldsAreInValid(
        RefreshPaymentMetadataCommandValidator validator
    )
    {
        var actual = validator.Validate(new RefreshPaymentMetadataCommand());

        actual.IsValid().Should().BeFalse();
    }
    
    [Test, MoqAutoData]
    public void ThenFalseIsReturnedWhenAllFieldsArentPopulatedAndTheErrorDictionaryIsPopulated(
        RefreshPaymentMetadataCommandValidator validator
    )
    {
        var actual = validator.Validate(new RefreshPaymentMetadataCommand());

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("AccountId", "AccountId has not been supplied"));
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("PaymentId", "PaymentId has not been supplied"));
    }
}