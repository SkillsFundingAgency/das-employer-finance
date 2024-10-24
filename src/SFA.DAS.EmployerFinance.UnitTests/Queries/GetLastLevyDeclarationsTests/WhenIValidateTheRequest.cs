using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLastLevyDeclarationsTests
{
    class WhenIValidateTheRequest
    {
        private GetLastLevyDeclarationValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetLastLevyDeclarationValidator();
        }

        [Test]
        public void ThenTheRequestIsValidWhenAllFieldsArePopulated()
        {
            //Arrange
            var actual = _validator.Validate(new GetLastLevyDeclarationQuery {EmpRef = "asdasd"});

            //Act
            actual.IsValid().Should().BeTrue();
        }

        [Test]
        public void ThenTheRequestIsNotValidWhenTheFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
        {
            //Arrange
            var actual = _validator.Validate(new GetLastLevyDeclarationQuery());

            //Act
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("EmpRef", "EmpRef has not been supplied")));
        }
    }
}
