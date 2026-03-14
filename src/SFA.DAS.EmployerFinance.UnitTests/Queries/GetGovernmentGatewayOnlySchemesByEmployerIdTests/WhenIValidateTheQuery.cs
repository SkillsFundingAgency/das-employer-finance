using SFA.DAS.EmployerFinance.Queries.GetGovernmentGatewayOnlySchemesByEmployerId;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetGovernmentGatewayOnlySchemesByEmployerIdTests
{
    public class WhenIValidateTheQuery
    {
        private GetGovernmentGatewayOnlySchemesByEmployerIdValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetGovernmentGatewayOnlySchemesByEmployerIdValidator();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheAccountIdIsNotSupplied()
        {
            //Act
            var actual = _validator.Validate(new GetGovernmentGatewayOnlySchemesByEmployerIdQuery { AccountId = 0 });

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("AccountId", "AccountId has not been supplied"));
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheAccountIdIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetGovernmentGatewayOnlySchemesByEmployerIdQuery { AccountId = 12345 });

            //Assert
            actual.IsValid().Should().BeTrue();
        }
    }
}
