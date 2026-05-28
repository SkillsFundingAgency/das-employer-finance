using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

public class WhenIGetExistingPeriod12LevyDeclarations
{
    private FinanceLevyDeclarationsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<LevyDeclarationOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();

        var orchestrator = new LevyDeclarationOrchestrator(_mediator.Object, _logger.Object);
        _controller = new FinanceLevyDeclarationsController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Ok_With_Declarations()
    {
        var empRef = "123/AB12345";
        var declarations = new List<ExistingPeriod12LevyDeclarationResult>
        {
            new()
            {
                Id = "1",
                LevyDueYtd = 50m,
                SubmissionDate = new DateTime(2026, 3, 1),
                PayrollYear = "25-26",
                PayrollMonth = 12,
                SubmissionId = 100
            }
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetExistingPeriod12LevyDeclarationsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(declarations);

        var result = await _controller.GetExistingPeriod12LevyDeclarations(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeAssignableTo<List<ExistingPeriod12LevyDeclarationResult>>();
        ((List<ExistingPeriod12LevyDeclarationResult>)result.Value!).Should().BeEquivalentTo(declarations);
    }
}
