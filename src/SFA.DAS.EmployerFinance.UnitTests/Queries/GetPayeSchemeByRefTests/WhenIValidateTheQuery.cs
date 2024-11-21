using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetPayeSchemeByRefTests
{
    public class WhenIValidateTheQuery
    {
        private GetPayeSchemeByRefValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetPayeSchemeByRefValidator();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheHashedIdIsntPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeByRefQuery { Ref = "ABC/123" });

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("Ref", "HashedAccountId has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheRefIsntPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeByRefQuery { HashedAccountId = "ABC123" });

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("Ref", "PayeSchemeRef has not been supplied")));
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheIdHasBeenPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeByRefQuery
            {
                HashedAccountId = "ABC123",
                Ref = "ABC/123"
            });

            //Assert
            actual.IsValid().Should().BeTrue();
        }
    }
}
