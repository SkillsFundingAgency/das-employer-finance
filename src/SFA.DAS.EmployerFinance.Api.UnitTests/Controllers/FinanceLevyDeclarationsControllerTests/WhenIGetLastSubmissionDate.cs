using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

public class WhenIGetLastSubmissionDate
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
    public async Task Then_Returns_Ok_With_SubmissionDate_Minus_One_Day()
    {
        var empRef = "123/AB12345";
        var submissionDate = new DateTime(2026, 4, 10);

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLastLevyDeclarationResponse
            {
                Transaction = new DasDeclaration { SubmissionDate = submissionDate }
            });

        var result = await _controller.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var responseObject = result.Value as LastSubmissionDateResult;
        responseObject.Should().NotBeNull();
        responseObject!.LastSumissionDate.Should().Be(submissionDate.AddDays(-1));
    }

    [Test]
    public async Task Then_Returns_Ok_With_Null_When_Declaration_Does_Not_Exist()
    {
        var empRef = "123/AB12345";

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLastLevyDeclarationQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetLastLevyDeclarationResponse)null!);

        var result = await _controller.GetLastSubmissionDate(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var responseObject = result.Value as LastSubmissionDateResult;
        responseObject.Should().NotBeNull();
        responseObject!.LastSumissionDate.Should().BeNull();
    }
}
