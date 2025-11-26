using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Queries.GetAccounts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIGetAccounts
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

        _mapper = new Mapper(new MapperConfiguration(c =>
        {
            c.AddProfile<AccountMappings>();
        }));

        var orchestrator = new FinanceOrchestrator(_mediator.Object, _logger.Object, _mapper, _encodingService.Object);
        _employerAccountsController = new EmployerAccountsController(orchestrator);
    }

    [Test]
    public async Task ThenReturnOkWithAccountsResult()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;

        var expectedResponse = new GetAccountsResponse
        {
            Accounts =
            [
                new() { HashedId = "1", Name = "Account 1" },
                new() { HashedId = "2", Name = "Account 2" }
            ]
        };

        _mediator
            .Setup(x => x.Send(It.Is<GetAccountsRequest>(q =>
                    q.PageNumber == pageNumber && q.PageSize == pageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employerAccountsController.GetAccounts(pageNumber, pageSize) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as GetAccountsResponse;
        responseObject.Should().NotBeNull();
        responseObject.Accounts!.Should().HaveCount(2);
    }

    [Test]
    public async Task ThenReturnNotFoundIfResultIsNull()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountsResponse)null!);

        // Act
        var result = await _employerAccountsController.GetAccounts(pageNumber, pageSize);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
