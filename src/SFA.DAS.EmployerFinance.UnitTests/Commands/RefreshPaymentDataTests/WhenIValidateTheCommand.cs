﻿using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshPaymentDataTests
{
    public class WhenIValidateTheCommand
    {
        private RefreshPaymentDataCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new RefreshPaymentDataCommandValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsAreValid()
        {
            //Act
            var actual = _validator.Validate(new RefreshPaymentDataCommand
            {
                AccountId = 123123,
                PeriodEnd = "123"
            });

            //Assert
            Assert.IsTrue(actual.IsValid());
        }

        [Test]
        public void ThenFalseIsReturnedWhenAllFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new RefreshPaymentDataCommand());

            //Assert
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string,string>("AccountId", "AccountId has not been supplied"), actual.ValidationDictionary );
            Assert.Contains(new KeyValuePair<string,string>("PeriodEnd", "PeriodEnd has not been supplied"), actual.ValidationDictionary );
        }
    }
}
