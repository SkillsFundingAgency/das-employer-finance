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
            Assert.IsTrue(result.IsValid());
            Assert.IsFalse(result.IsUnauthorized);
        }


        [Test]
        public async Task ThenTheDictionaryIsPopulatedWithValidationErrors()
        {
            //Act
            var result = await _validator.ValidateAsync(new GetEmployerAccountHashedQuery());

            //Assert
            Assert.IsFalse(result.IsValid());
            Assert.Contains(new KeyValuePair<string, string>("UserId", "UserId has not been supplied"), result.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied"), result.ValidationDictionary);
        }

    }
}
