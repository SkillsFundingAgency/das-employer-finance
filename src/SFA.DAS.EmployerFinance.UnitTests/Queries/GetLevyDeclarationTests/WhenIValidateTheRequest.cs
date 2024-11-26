using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevyDeclarationTests
{
    public class WhenIValidateTheRequest
    {
        private GetLevyDeclarationValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetLevyDeclarationValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new GetLevyDeclarationRequest { HashedAccountId = "12587" });

            //Assert
            actual.IsValid().Should().BeTrue();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheFieldsArentPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetLevyDeclarationRequest { HashedAccountId = "" });

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
        }
    }
}
