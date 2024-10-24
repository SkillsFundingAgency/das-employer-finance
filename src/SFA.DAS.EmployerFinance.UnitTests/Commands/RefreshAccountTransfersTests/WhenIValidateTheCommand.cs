using SFA.DAS.EmployerFinance.Commands.RefreshAccountTransfers;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshAccountTransfersTests
{
    public class WhenIValidateTheCommand
    {
        private RefreshAccountTransfersCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new RefreshAccountTransfersCommandValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsAreValid()
        {
            //Act
            var actual = _validator.Validate(new RefreshAccountTransfersCommand
            {
                ReceiverAccountId = 123123,
                PeriodEnd = "123"
            });

            //Assert
            (actual.IsValid()).Should().BeTrue();
        }

        [Test]
        public void ThenFalseIsReturnedWhenAllFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new RefreshAccountTransfersCommand());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("ReceiverAccountId", "ReceiverAccountId has not been supplied")));
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("PeriodEnd", "PeriodEnd has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedWhenPeriodEndIsEmpty()
        {
            //Act
            var actual = _validator.Validate(new RefreshAccountTransfersCommand
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
            var actual = _validator.Validate(new RefreshAccountTransfersCommand
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
