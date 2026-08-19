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

    [Test]
    public void ThenFalseIsReturnedWhenEmployerReferenceIsInvalid()
    {
        var actual = _validator.Validate(new PersistEnglishFractionsCommand
        {
            EmployerReference = "778<>/GDS00004",
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = "778<>/GDS00004", DateCalculated = new DateTime(2025, 01, 01), Amount = 0.6m }
            }
        });

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>(
            "EmployerReference",
            "EmployerReference must be a valid PAYE reference"));
    }

    [Test]
    public void ThenEmployerReferenceAndFractionEmpRefsAreNormalisedWhenValidWithoutSlash()
    {
        var command = new PersistEnglishFractionsCommand
        {
            EmployerReference = "123ab456",
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<DasEnglishFraction>
            {
                new() { EmpRef = "123ab456", DateCalculated = new DateTime(2025, 01, 01), Amount = 0.6m }
            }
        };

        var actual = _validator.Validate(command);

        actual.IsValid().Should().BeTrue();
        command.EmployerReference.Should().Be("123/AB456");
        command.Fractions[0].EmpRef.Should().Be("123/AB456");
    }
}

