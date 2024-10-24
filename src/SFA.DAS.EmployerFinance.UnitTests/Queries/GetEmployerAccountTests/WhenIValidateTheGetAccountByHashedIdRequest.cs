using SFA.DAS.EmployerFinance.Queries.GetEmployerAccount;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEmployerAccountTests
{
    public class WhenIValidateTheGetAccountByHashedIdRequest
    {
        private GetEmployerAccountByHashedIdValidator _validator;

        private const string ExpectedHashedId = "4567";
        private const string ExpectedUserId = "asdf4660";

        [SetUp]
        public void Arrange()
        {
            _validator = new GetEmployerAccountByHashedIdValidator();
        }

        [Test]
        public async Task ThenTheResultIsValidWhenAllFieldsArePopulatedAndTheUserIsPartOfTheAccount()
        {
            //Act
            var result = await _validator.ValidateAsync(new GetEmployerAccountHashedQuery { HashedAccountId = ExpectedHashedId, UserId = ExpectedUserId });

            //Assert
            result.IsValid().Should().BeTrue();
            result.IsUnauthorized.Should().BeFalse();
        }


        [Test]
        public async Task ThenTheDictionaryIsPopulatedWithValidationErrors()
        {
            //Act
            var result = await _validator.ValidateAsync(new GetEmployerAccountHashedQuery());

            //Assert
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("UserId", "UserId has not been supplied")));
            result.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
        }

    }
}
