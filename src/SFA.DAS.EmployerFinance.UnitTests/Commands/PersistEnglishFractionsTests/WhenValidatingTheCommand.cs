using SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.PersistEnglishFractionsTests;

public class WhenValidatingTheCommand
{
    private PersistEnglishFractionsCommandValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new PersistEnglishFractionsCommandValidator();
    }

    [Test]
    public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
    {
        //Act
        var actual = _validator.Validate(new PersistEnglishFractionsCommand
        {
            EmployerReference = "123/AB456",
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = "123/AB456", DateCalculated = new DateTime(2025, 01, 01), Amount = 0.6m }
            }
        });

        //Assert
        actual.Should().NotBeNull();
        actual.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenFalseIsReturnedWhenFieldsAreMissing()
    {
        //Act
        var actual = _validator.Validate(new PersistEnglishFractionsCommand());

        //Assert
        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmployerReference", "EmployerReference has not been supplied"));
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("DateCalculated", "DateCalculated has not been supplied"));
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("Fractions", "Fractions payload is required."));
    }

    [Test]
    public void ThenFalseIsReturnedWhenFractionsAreEmpty()
    {
        var actual = _validator.Validate(new PersistEnglishFractionsCommand
        {
            EmployerReference = "123/AB456",
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<DasEnglishFraction>()
        });

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("Fractions", "Fractions payload is required."));
    }
}
