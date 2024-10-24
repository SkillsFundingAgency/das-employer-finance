using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.CreateEnglishFractionCalculationDateCommandTests
{
    public class WhenIValidateTheCommand
    {
        private CreateEnglishFractionCalculationDateCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new CreateEnglishFractionCalculationDateCommandValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllTheFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new CreateEnglishFractionCalculationDateCommand
            {
                DateCalculated = new DateTime(2016, 01, 01)
            });

            //Assert
            (actual.IsValid()).Should().BeTrue();
        }

        [Test]
        public void ThenFalseIsReturnedAndTheErrorDictionaryIsPopulatedWhenTheFieldsArentPopulated()
        {
            //Act
            var actual = _validator.Validate(new CreateEnglishFractionCalculationDateCommand());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("DateCalculated", "DateCalculated has not been supplied")));
        }
    }
}
