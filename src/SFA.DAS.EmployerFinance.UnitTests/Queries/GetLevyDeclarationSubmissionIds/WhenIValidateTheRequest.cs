using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevyDeclarationSubmissionIdsTests;

public class WhenIValidateTheRequest
{
    private GetLevyDeclarationSubmissionIdsValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetLevyDeclarationSubmissionIdsValidator();
    }

    [Test]
    public void ThenTheRequestIsValidWhenAllFieldsArePopulated()
    {
        var actual = _validator.Validate(new GetLevyDeclarationSubmissionIdsQuery { EmpRef = "asdasd" });

        actual.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenTheRequestIsNotValidWhenTheFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
    {
        var actual = _validator.Validate(new GetLevyDeclarationSubmissionIdsQuery());

        actual.IsValid().Should().BeFalse();
        actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmpRef", "EmpRef has not been supplied"));
    }
}
