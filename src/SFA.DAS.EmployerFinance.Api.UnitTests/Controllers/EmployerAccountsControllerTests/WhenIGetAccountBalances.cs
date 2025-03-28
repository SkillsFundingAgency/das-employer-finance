﻿using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIGetAccountBalances
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
    public async Task ThenReturnTheAccountBalance()
    {
        //Arrange
        var hashedAccountIds = new List<string> { "ABC123", "XYZ456" };

        var accountBalancesResponse = new GetAccountBalancesResponse {
            Accounts = new List<Models.Account.AccountBalance> { new Models.Account.AccountBalance { AccountId = 1, Balance = 10000 }, 
                new Models.Account.AccountBalance { AccountId = 2, Balance = 20000 } }
        };
            
        _mediator.Setup(x => x.Send(It.Is<GetAccountBalancesRequest>(q => q.AccountIds == It.IsAny<List<long>>()), It.IsAny<CancellationToken>())).ReturnsAsync(accountBalancesResponse);

        //Act
        var response = await _employerAccountsController.GetAccountBalances(hashedAccountIds);

        //Assert
        response.Should().NotBeNull();
    }
}