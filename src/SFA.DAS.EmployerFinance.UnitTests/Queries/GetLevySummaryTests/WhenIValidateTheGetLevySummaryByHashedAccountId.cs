using SFA.DAS.EmployerFinance.Queries.GetLevySummary;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevySummaryTests;

internal class WhenIValidateTheGetLevySummaryByHashedAccountId
{
    private GetLevySummaryQueryValidator _validator;

    private const string ExpectedHashedId = "4567";

    [SetUp]
    public void Arrange()
    {
        _validator = new GetLevySummaryQueryValidator();
    }

    [Test]
    public async Task ThenTheResultIsValidWhenAllFieldsArePopulatedAndTheUserIsPartOfTheAccount()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetLevySummaryQuery(ExpectedHashedId));

        //Assert
        result.IsValid().Should().BeTrue();
        result.IsUnauthorized.Should().BeFalse();
    }


    [Test]
    public async Task ThenTheDictionaryIsPopulatedWithValidationErrors()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetLevySummaryQuery(null));

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
    }
}