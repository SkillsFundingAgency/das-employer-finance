using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevySummaryTests;

internal class WhenIValidateTheGetLevySummaryByHashedAccountId
{
    private GetLevySummaryByHashedAccountIdQueryValidator _validator;

    private const string ExpectedHashedId = "4567";

    [SetUp]
    public void Arrange()
    {
        _validator = new GetLevySummaryByHashedAccountIdQueryValidator();
    }

    [Test]
    public async Task ThenTheResultIsValidWhenAllFieldsArePopulatedAndTheUserIsPartOfTheAccount()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedId));

        //Assert
        result.IsValid().Should().BeTrue();
        result.IsUnauthorized.Should().BeFalse();
    }


    [Test]
    public async Task ThenTheDictionaryIsPopulatedWithValidationErrors()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetLevySummaryByHashedAccountIdQuery(null));

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
    }
}