using SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.UpdatePayeInformationTests
{
    public class WhenValidatingTheCommand
    {
        private UpdatePayeInformationValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new UpdatePayeInformationValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //act
            var actual = _validator.Validate(new UpdatePayeInformationCommand {PayeRef = "1234rf"});

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeTrue();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheFieldsArentPopulatedAndTheErrorDictionaryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new UpdatePayeInformationCommand());

            //Assert
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().Contain((new KeyValuePair<string, string>("PayeRef", "PayeRef has not been supplied")));
        }
    }
}