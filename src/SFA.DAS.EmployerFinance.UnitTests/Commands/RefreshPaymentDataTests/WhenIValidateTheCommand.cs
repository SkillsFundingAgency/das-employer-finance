using SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

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
            actual.IsValid().Should().BeTrue();
        }

        [Test]
        public void ThenFalseIsReturnedWhenAllFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new RefreshPaymentDataCommand());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should()
                .ContainKey("AccountId")
                .WhoseValue
                .Should().Be("AccountId has not been supplied");
            actual.ValidationDictionary.Should()
                .ContainKey("PeriodEnd")
                .WhoseValue
                .Should().Be("PeriodEnd has not been supplied");
        }
    }
}
