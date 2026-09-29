using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevySummaryTests;

internal class WhenIValidateTheGetLevySummaryByAccountId
{
    private GetLevySummaryByAccountIdQueryValidator _validator;

    private const long ExpectedAccountId = 4567;

    [SetUp]
    public void Arrange()
    {
        _validator = new GetLevySummaryByAccountIdQueryValidator();
    }

    [Test]
    public async Task ThenTheResultIsValidWhenAllFieldsArePopulatedAndTheUserIsPartOfTheAccount()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetLevySummaryByAccountIdQuery(ExpectedAccountId));

        //Assert
        result.IsValid().Should().BeTrue();
        result.IsUnauthorized.Should().BeFalse();
    }


    [Test]
    public async Task ThenTheDictionaryIsPopulatedWithValidationErrors()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetLevySummaryByAccountIdQuery(0));

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("AccountId", "AccountId has not been supplied")));
    }
}