using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Api.Mappings;
using SFA.DAS.EmployerFinance.Models.Paye;
using SFA.DAS.EmployerFinance.Queries.GetGovernmentGatewayOnlySchemesByEmployerId;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIGetGovernmentGatewayPayeSchemes
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
    public async Task ThenReturnOkWithPayeSchemes()
    {
        // Arrange
        var accountId = 12345L;

        var schemes = new List<Paye>
        {
            new() { EmpRef = "123/AB123" },
            new() { EmpRef = "456/CD456" }
        };

        var expectedResponse = new GetGovernmentGatewayOnlySchemesByEmployerIdResponse
        {
            Schemes = schemes
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>(q => q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employerAccountsController.GetGovernmentGatewayOnlyPayeSchemes(accountId, "government-gateway") as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as List<PayeScheme>;
        responseObject.Should().NotBeNull();
        responseObject!.Select(x => x.EmpRef).Should().BeEquivalentTo(new[] { "123/AB123", "456/CD456" });
    }

    [Test]
    public async Task ThenReturnOkWithEmptyListWhenNoSchemes()
    {
        // Arrange
        var accountId = 12345L;

        var expectedResponse = new GetGovernmentGatewayOnlySchemesByEmployerIdResponse
        {
            Schemes = new List<Paye>()
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>(q => q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _employerAccountsController.GetGovernmentGatewayOnlyPayeSchemes(accountId, "government-gateway") as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var responseObject = result.Value as List<PayeScheme>;
        responseObject.Should().NotBeNull();
        responseObject!.Count.Should().Be(0);
    }

    [Test]
    public async Task ThenReturnNotFoundWhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = 999L;

        _mediator
            .Setup(x => x.Send(
                It.Is<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>(q => q.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetGovernmentGatewayOnlySchemesByEmployerIdResponse)null!);

        // Act
        var result = await _employerAccountsController.GetGovernmentGatewayOnlyPayeSchemes(accountId, "government-gateway");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
