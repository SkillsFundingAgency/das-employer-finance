using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetExistingPeriod12LevyDeclarationsTests;

public class WhenIValidateTheRequest
{
    private GetExistingPeriod12LevyDeclarationsValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetExistingPeriod12LevyDeclarationsValidator();
    }

    [Test]
    public void ThenTheRequestIsValidWhenAllFieldsArePopulated()
    {
        var actual = _validator.Validate(new GetExistingPeriod12LevyDeclarationsQuery { EmpRef = "asdasd" });

        actual.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenTheRequestIsNotValidWhenTheFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
    {
        var actual = _validator.Validate(new GetExistingPeriod12LevyDeclarationsQuery());

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmpRef", "EmpRef has not been supplied"));
    }
}
