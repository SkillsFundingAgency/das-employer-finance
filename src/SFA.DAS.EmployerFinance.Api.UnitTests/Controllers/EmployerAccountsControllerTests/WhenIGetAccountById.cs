using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Queries.GetAccount;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIGetAccountById
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

        var orchestrator = new FinanceOrchestrator(
            _mediator.Object,
            _logger.Object,
            _mapper,
            _encodingService.Object);

        _employerAccountsController = new EmployerAccountsController(orchestrator);
    }

    [Test]
    public async Task ThenReturnOkWithAccountResult()
    {
        // Arrange
        var accountId = 12345L;

        var expectedResponse = new GetAccountByIdResponse
        {
            Account = new Models.Account.Account
            {
                //HashedId = accountId,
                Name = "Test Account"
            }
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetAccountByIdRequest>(q => q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employerAccountsController.GetAccountById(accountId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as Account;
        responseObject.Should().NotBeNull();
        responseObject.Name.Should().Be("Test Account");
    }

    [Test]
    public async Task ThenReturnNotFoundIfResultIsNull()
    {
        // Arrange
        var accountId = 999L;

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountByIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetAccountByIdResponse)null!);

        // Act
        var result = await _employerAccountsController.GetAccountById(accountId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

}
