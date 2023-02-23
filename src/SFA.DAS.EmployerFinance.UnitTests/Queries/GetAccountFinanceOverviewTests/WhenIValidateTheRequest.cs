﻿using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests
{
    public class WhenIValidateTheRequest
    {
        private Mock<IAuthorizationService> _authorizationService;
        private GetAccountFinanceOverviewQueryValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _validator = new GetAccountFinanceOverviewQueryValidator(_authorizationService.Object);
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
