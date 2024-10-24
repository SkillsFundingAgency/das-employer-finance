using SFA.DAS.EmployerFinance.Commands.CreateTransferTransactions;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.CreateTransferTransactionsTests
{
    public class WhenIValidateTheCommand
    {
        private CreateTransferTransactionsCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateTransferTransactionsCommandValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsAreValid()
        {
            //Act
            var actual = _validator.Validate(new CreateTransferTransactionsCommand
            {
                ReceiverAccountId = 123123,
                PeriodEnd = "123"
            });

            //Assert
            actual.IsValid().Should().BeTrue();
        }

        [Test]
        public void ThenFalseIsReturnedWhenAllFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new CreateTransferTransactionsCommand());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("ReceiverAccountId", "ReceiverAccountId has not been supplied")));
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("PeriodEnd", "PeriodEnd has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedWhenPeriodEndIsEmpty()
        {
            //Act
            var actual = _validator.Validate(new CreateTransferTransactionsCommand
            {
                ReceiverAccountId = 123123,
                PeriodEnd = string.Empty
            });

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("PeriodEnd", "PeriodEnd has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedWhenAccountIdIsNegative()
        {
            //Act
            var actual = _validator.Validate(new CreateTransferTransactionsCommand
            {
                ReceiverAccountId = -123123,
                PeriodEnd = "123"
            });

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("ReceiverAccountId", "ReceiverAccountId cannot be negative")));
        }
    }
}
