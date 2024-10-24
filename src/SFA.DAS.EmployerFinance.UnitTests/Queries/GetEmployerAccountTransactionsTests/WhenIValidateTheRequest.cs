using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEmployerAccountTransactionsTests;

public class WhenIValidateTheRequest
{
    private GetEmployerAccountTransactionsValidator _validator;        

    [SetUp]
    public void Arrange()
    {
        _validator = new GetEmployerAccountTransactionsValidator();
    }

    [Test]
    public async Task ThenItIsValidIfAllFieldsArePopUlated()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetEmployerAccountTransactionsQuery { HashedAccountId = "AD1" });

        //Assert
        result.IsValid().Should().BeTrue();
    }        


    [Test]
    public async Task ThenIfTheFieldsArentPopulatedThenTheResultIsNotValidAndTheErrorDictionaryIsPopulated()
    {
        //Act
        var result = await _validator.ValidateAsync(new GetEmployerAccountTransactionsQuery());

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
    }
}