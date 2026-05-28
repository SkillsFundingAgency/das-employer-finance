using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

public class WhenIGetSubmissionIds
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
    public async Task Then_Returns_Ok_With_Submission_Id_List()
    {
        var empRef = "123/AB12345";
        var ids = new List<long> { 1001, 1002 };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLevyDeclarationSubmissionIdsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ids);

        var result = await _controller.GetSubmissionIds(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeAssignableTo<List<string>>();
        ((List<string>)result.Value!).Should().BeEquivalentTo(new List<string> { "1001", "1002" }, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Then_Returns_Ok_With_Empty_List_When_None_Exist()
    {
        var empRef = "123/AB12345";
        var empty = new List<long>();

        _mediator
            .Setup(x => x.Send(
                It.Is<GetLevyDeclarationSubmissionIdsQuery>(q => q.EmpRef == empRef),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(empty);

        var result = await _controller.GetSubmissionIds(empRef) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        ((List<string>)result.Value!).Should().BeEmpty();
    }
}
