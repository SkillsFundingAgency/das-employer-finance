using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.CreateNewPeriodEndTests
{
    public class WhenIValidateTheCommand
    {
        private CreateNewPeriodEndCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateNewPeriodEndCommandValidator();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new CreateNewPeriodEndCommand());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should()
                .ContainKey("NewPeriodEnd")
                .WhichValue
                .Should()
                .Be("NewPeriodEnd has not been supplied");
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new CreateNewPeriodEndCommand {NewPeriodEnd = new PeriodEnd() });

            //Assert
            actual.IsValid().Should().BeTrue();
        }
    }
}
