using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;
using SFA.DAS.EmployerFinance.Queries.GetAccounts;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIGetAccountPaymentIds
{
    private EmployerAccountsController _employerAccountsController;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<FinanceOrchestrator>> _logger;
    private Mapper _mapper;
    private Mock<IEncodingService> _encodingService;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<FinanceOrchestrator>>();
        _encodingService = new Mock<IEncodingService>();

        var orchestrator = new FinanceOrchestrator(_mediator.Object, _logger.Object, _mapper, _encodingService.Object);
        _employerAccountsController = new EmployerAccountsController(orchestrator);
    }

    [Test]
    public async Task ThenReturnOkWithPaymentIdsResult()
    {
        // Arrange
        var accountId = 12345L;

        var expectedIds = new List<Guid>
    {
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid()
    };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetAccountPaymentIdsRequest>(q => q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAccountPaymentIdsResponse
            {
                PaymentIds = expectedIds
            });

        // Act
        var result = await _employerAccountsController.GetAccountPaymentIds(accountId)
                     as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as GetAccountPaymentIdsResponse;
        responseObject.Should().NotBeNull();
        responseObject!.PaymentIds.Should().HaveCount(3);
        responseObject.PaymentIds.Should().BeEquivalentTo(expectedIds);
    }


    [Test]
    public async Task ThenReturnNotFoundIfResultIsNull()
    {
        // Arrange
        var accountId = 12345L;

        _mediator
            .Setup(x => x.Send(
                It.IsAny<GetAccountPaymentIdsRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountPaymentIdsResponse)null!);

        // Act
        var result = await _employerAccountsController.GetAccountPaymentIds(accountId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
