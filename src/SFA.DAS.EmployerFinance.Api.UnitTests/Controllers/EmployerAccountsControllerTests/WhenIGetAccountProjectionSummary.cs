using System.Net;
using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummary;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests
{
    public class WhenIGetAccountProjectionSummary
    {
        private EmployerAccountsController _employerAccountsController;
        private Mock<IMediator> _mediator;
        private Mock<ILogger<FinanceOrchestrator>> _logger;
        private Mock<IMapper> _mapper;
        private Mock<IEncodingService> _encodingService;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<FinanceOrchestrator>>();
            _mapper = new Mock<IMapper>();
            _encodingService = new Mock<IEncodingService>();

            var orchestrator = new FinanceOrchestrator(_mediator.Object, _logger.Object, _mapper.Object, _encodingService.Object);
            _employerAccountsController = new EmployerAccountsController(orchestrator);
        }

        [Test]
        public async Task ThenReturnTheAccountProjectionSummary()
        {
            //Arrange
            var accountId = 123;

            var summaryResult = new GetAccountProjectionSummaryResult
            {
                AccountId = accountId,
                FundsIn = 1000
            };

            _mediator.Setup(x => x.Send(It.IsAny<GetAccountProjectionSummaryQuery>(), default)).ReturnsAsync(summaryResult);

            //Act
            var response = await _employerAccountsController.GetAccountProjectionSummary(accountId);

            //Assert
            response.Should().NotBeNull();
        }

        [Test]
        public async Task ThenReturn_NotFound_If_Null()
        {
            //Arrange
            var accountId = 123;

            _mediator.Setup(x => x.Send(It.IsAny<GetAccountProjectionSummaryQuery>(), default)).ReturnsAsync(() => null);

            //Act
            var controllerResult = await _employerAccountsController.GetAccountProjectionSummary(accountId) as NotFoundResult;

            //Assert
            controllerResult.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}