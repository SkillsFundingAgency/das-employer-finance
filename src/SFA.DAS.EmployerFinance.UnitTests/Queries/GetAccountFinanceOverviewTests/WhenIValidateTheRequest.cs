using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests
{
    public class WhenIValidateTheRequest
    {
        
        private GetAccountFinanceOverviewQueryValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetAccountFinanceOverviewQueryValidator();
        }

        [Test]
        public async Task ThenTrueIsReturnedWhenAllFieldsArePopulatedAndTheMemberIsPartOfTheAccount()
        {
            //Act
            var actual = await _validator.ValidateAsync(new GetAccountFinanceOverviewQuery
                {
                    AccountId = 10
                });

            //Assert
            Assert.IsTrue(actual.IsValid());
        }

    }
}
