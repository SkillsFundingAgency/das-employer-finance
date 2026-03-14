using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Paye;
using SFA.DAS.EmployerFinance.Queries.GetGovernmentGatewayOnlySchemesByEmployerId;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetGovernmentGatewayOnlySchemesByEmployerIdTests
{
    public class WhenIGetGovernmentGatewayOnlySchemes : QueryBaseTest<GetGovernmentGatewayOnlySchemesByEmployerIdHandler, GetGovernmentGatewayOnlySchemesByEmployerIdQuery, GetGovernmentGatewayOnlySchemesByEmployerIdResponse>
    {
        private Mock<IPayeRepository> _payeRepository;
        private Mock<IEmployerAccountRepository> _employerAccountRepository;

        public override GetGovernmentGatewayOnlySchemesByEmployerIdQuery Query { get; set; }
        public override GetGovernmentGatewayOnlySchemesByEmployerIdHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>> RequestValidator { get; set; }

        private const long ExpectedAccountId = 12345;
        private List<Paye> _expectedSchemes;

        [SetUp]
        public void Arrange()
        {
            SetUp();

            Query = new GetGovernmentGatewayOnlySchemesByEmployerIdQuery
            {
                AccountId = ExpectedAccountId
            };

            _expectedSchemes = new List<Paye>
            {
                new() { EmpRef = "123/AB123", AccountId = ExpectedAccountId, Name = "Scheme 1", Aorn = "A1" },
                new() { EmpRef = "456/CD456", AccountId = ExpectedAccountId, Name = "Scheme 2", Aorn = "A2" }
            };

            _payeRepository = new Mock<IPayeRepository>();
            _payeRepository
                .Setup(x => x.GetGovernmentGatewayOnlySchemesByEmployerId(ExpectedAccountId))
                .ReturnsAsync(new PayeSchemes { SchemesList = _expectedSchemes });

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository
                .Setup(x => x.Get(ExpectedAccountId))
                .ReturnsAsync(new Account { Id = ExpectedAccountId, Name = "Test Account" });

            RequestHandler = new GetGovernmentGatewayOnlySchemesByEmployerIdHandler(
                RequestValidator.Object,
                _payeRepository.Object,
                _employerAccountRepository.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>()))
                .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _employerAccountRepository.Verify(x => x.Get(ExpectedAccountId), Times.Once);
            _payeRepository.Verify(x => x.GetGovernmentGatewayOnlySchemesByEmployerId(ExpectedAccountId), Times.Once);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>()))
                .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            actual.Schemes.Should().BeEquivalentTo(_expectedSchemes);
        }

        [Test]
        public async Task ThenNullIsReturnedWhenAccountDoesNotExist()
        {
            //Arrange
            _employerAccountRepository
                .Setup(x => x.Get(ExpectedAccountId))
                .ReturnsAsync((Account)null);

            RequestValidator.Setup(x => x.Validate(It.IsAny<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>()))
                .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            actual.Should().BeNull();
            _payeRepository.Verify(x => x.GetGovernmentGatewayOnlySchemesByEmployerId(It.IsAny<long>()), Times.Never);
        }
    }
}
