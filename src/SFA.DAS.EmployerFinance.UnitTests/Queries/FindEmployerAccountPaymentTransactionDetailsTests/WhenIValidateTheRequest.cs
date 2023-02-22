using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.EmployerUserRoles.Options;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.FindEmployerAccountPaymentTransactionDetailsTests
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
            Assert.IsTrue(actual.IsValid());
        }

        [Test]
        public async Task ThenFalseIsReturnedAndTheValidtionDictionaryIsPopulatedWhenFieldsArentSupplied()
        {
            //Act
            var actual = await _validator.ValidateAsync(new FindEmployerAccountLevyDeclarationTransactionsQuery());

            //Assert
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string,string>("FromDate", "From date has not been supplied"), actual.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string,string>("ToDate", "To date has not been supplied"), actual.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string,string>("HashedAccountId", "HashedAccountId has not been supplied"), actual.ValidationDictionary);
        }
    }
}
