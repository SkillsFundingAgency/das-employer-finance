using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEmployerAccountDetailTests;

public class WhenIValidateTheGetEmployerAccountDetailByHashedIdRequest
{
    private GetEmployerAccountDetailByHashedIdValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetEmployerAccountDetailByHashedIdValidator();
    }

    [Test]
    public async Task ThenTheResultIsValidWhenHashedAccountIdIsPopulated()
    {
        var result = await _validator.ValidateAsync(new GetEmployerAccountDetailByHashedIdQuery
        {
            HashedAccountId = "VW6B97"
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public async Task ThenTheDictionaryIsPopulatedWithValidationErrorsWhenHashedAccountIdIsMissing()
    {
        var result = await _validator.ValidateAsync(new GetEmployerAccountDetailByHashedIdQuery());

        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain(
            new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied"));
    }
}
