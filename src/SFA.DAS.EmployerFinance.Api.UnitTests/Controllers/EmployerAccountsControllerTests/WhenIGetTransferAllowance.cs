using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIGetTransferAllowance
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
    public async Task ThenReturnTheTransferAllowance()
    {
        //Arrange
        var hashedAccountId = "ABC123";

        var accountBalancesResponse = new GetTransferAllowanceResponse
        {
            TransferAllowance = new Models.Transfers.TransferAllowance { RemainingTransferAllowance = 10 }
        };
            
        _mediator.Setup(x => x.Send(It.Is<GetTransferAllowanceQuery>(q => q.AccountId == It.IsAny<long>()), It.IsAny<CancellationToken>())).ReturnsAsync(accountBalancesResponse);

        //Act
        var response = await _employerAccountsController.GetTransferAllowance(hashedAccountId);

        //Assert
        Assert.IsNotNull(response);
    }
}