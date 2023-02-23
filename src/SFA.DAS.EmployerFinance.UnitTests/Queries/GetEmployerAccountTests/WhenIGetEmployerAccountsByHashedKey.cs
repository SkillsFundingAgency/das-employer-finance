using SFA.DAS.EmployerFinance.Queries.GetEmployerAccount;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;
using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEmployerAccountTests
{
    public class WhenIGetEmployerAccountsByHashedKey : QueryBaseTest<GetEmployerAccountHashedHandler, GetEmployerAccountHashedQuery, GetEmployerAccountResponse>
    {
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        public override GetEmployerAccountHashedQuery Query { get; set; }
        public override GetEmployerAccountHashedHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetEmployerAccountHashedQuery>> RequestValidator { get; set; }
        private const string ExpectedUserId = "asdsa445";
        private const string ExpectedHashedId = "jfjfdjf444";
        private const long ExpectedAccountId = 32450;
        private Mock<IEncodingService> _encodingService;
        private Account ExpectedAccount;

        [SetUp]
        public void Arrange()
        {
            base.SetUp();

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(ExpectedHashedId,EncodingType.AccountId)).Returns(ExpectedAccountId);

            ExpectedAccount = new Account();

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository.Setup(x => x.Get(ExpectedAccountId)).ReturnsAsync(ExpectedAccount);
            
            RequestHandler = new GetEmployerAccountHashedHandler(_employerAccountRepository.Object, RequestValidator.Object, _encodingService.Object);
            Query = new GetEmployerAccountHashedQuery { HashedAccountId = ExpectedHashedId, UserId = ExpectedUserId };

        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(new GetEmployerAccountHashedQuery
            {
                HashedAccountId = ExpectedHashedId,
                UserId = ExpectedUserId
            }, CancellationToken.None);

            //Assert
            _employerAccountRepository.Verify(x => x.Get(ExpectedAccountId));
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var result = await RequestHandler.Handle(new GetEmployerAccountHashedQuery
            {
                HashedAccountId = ExpectedHashedId,
                UserId = ExpectedUserId
            }, CancellationToken.None);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreSame(ExpectedAccount, result.Account);
        }


        [Test]
        public void ThenIfValidationResponseIsUnauthorizedAnUnauthorizedAccessExceptionIsThrown()
        {
            //Arrange
            RequestValidator.Setup(x => x.ValidateAsync(It.IsAny<GetEmployerAccountHashedQuery>())).ReturnsAsync(new ValidationResult { IsUnauthorized = true });

            //Act Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await RequestHandler.Handle(new GetEmployerAccountHashedQuery(), CancellationToken.None));

        }
    }
}
