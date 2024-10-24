using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.FindEmployerAccountLevyDeclarationTransactionDetailsTests
{
    public class WhenIValidateTheRequest
    {
        private FindEmployerAccountLevyDeclarationTransactionsQueryValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new FindEmployerAccountLevyDeclarationTransactionsQueryValidator();
        }

        [Test]
        public async Task ThenTrueIsReturnedWhenAllFieldsArePopulatedAndTheMemberIsPartOfTheAccount()
        {
            //Act
            var actual = await _validator.ValidateAsync(new FindEmployerAccountLevyDeclarationTransactionsQuery
                {
                    HashedAccountId = "test",
                    FromDate = DateTime.Now.AddDays(-10),
                    ToDate = DateTime.Now.AddDays(-2)
                });

            //Assert
            actual.IsValid().Should().BeTrue();
        }

        [Test]
        public async Task ThenFalseIsReturnedAndTheValidtionDictionaryIsPopulatedWhenFieldsArentSupplied()
        {
            //Act
            var actual = await _validator.ValidateAsync(new FindEmployerAccountLevyDeclarationTransactionsQuery());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("FromDate", "From date has not been supplied")));
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("ToDate", "To date has not been supplied")));
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
        }

    }
}
