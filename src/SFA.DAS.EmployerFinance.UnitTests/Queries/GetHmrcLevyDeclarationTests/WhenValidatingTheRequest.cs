using SFA.DAS.EmployerFinance.Queries.GetHMRCLevyDeclaration;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetHmrcLevyDeclarationTests
{
    public class WhenValidatingTheRequest
    {
        private GetHMRCLevyDeclarationQueryValidator _getHMRCLevyDeclarationQueryValidator;

        [SetUp]
        public void Arrange()
        {
            _getHMRCLevyDeclarationQueryValidator = new GetHMRCLevyDeclarationQueryValidator();
        }

        [Test]
        public void ThenTheDicitionaryIsPopulatedWhenTheIdIsMissing()
        {
            //Act
            var actual = _getHMRCLevyDeclarationQueryValidator.Validate(new GetHMRCLevyDeclarationQuery());

            //Assert
            actual.Should().NotBeNull();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("EmpRef", "The EmpRef field has not been supplied")));
        }

        [Test]
        public void ThenTheDictionaryIsNotPopulatedIfAllFieldsAreSupplied()
        {
            //Act
            var actual = _getHMRCLevyDeclarationQueryValidator.Validate(new GetHMRCLevyDeclarationQuery {EmpRef = "123456"});

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeTrue();
        }
    }
}
