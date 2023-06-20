using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetPayeSchemeByRefTests
{
    public class WhenIGetAPayeScheme : QueryBaseTest<GetPayeSchemeByRefHandler, GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>
    {
        private Mock<IPayeRepository> _payeRepository;
        public override GetPayeSchemeByRefQuery Query { get; set; }
        public override GetPayeSchemeByRefHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetPayeSchemeByRefQuery>> RequestValidator { get; set; }
        private Mock<IEncodingService> _encodingService;
        private const long ExpectedAccountId = 342905843;

        private PayeSchemeView _expectedPayeScheme;

        [SetUp]
        public void Arrange()
        {
            SetUp();

            _expectedPayeScheme = new PayeSchemeView();

            Query = new GetPayeSchemeByRefQuery
            {
                HashedAccountId = "ABC123",
                Ref = "ABC/123"
            };

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(Query.HashedAccountId,EncodingType.AccountId)).Returns(ExpectedAccountId);

            _payeRepository = new Mock<IPayeRepository>();
            _payeRepository.Setup(x => x.GetPayeForAccountByRef(ExpectedAccountId, Query.Ref)).ReturnsAsync(_expectedPayeScheme);
            
            RequestHandler = new GetPayeSchemeByRefHandler(RequestValidator.Object,_payeRepository.Object, _encodingService.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<GetPayeSchemeByRefQuery>())).Returns(new ValidationResult {ValidationDictionary = new Dictionary<string, string>()});

            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _payeRepository.Verify(x => x.GetPayeForAccountByRef(ExpectedAccountId, Query.Ref), Times.Once);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<GetPayeSchemeByRefQuery>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            Assert.AreSame(_expectedPayeScheme, actual.PayeScheme);
        }
    }
}
