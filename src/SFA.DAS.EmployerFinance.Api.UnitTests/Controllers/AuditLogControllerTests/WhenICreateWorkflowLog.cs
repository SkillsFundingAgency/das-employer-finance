using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AuditLogControllerTests;

public class WhenICreateWorkflowLog
{
    private AuditLogController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<AuditLogOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<AuditLogOrchestrator>>();

        _controller = new AuditLogController(new AuditLogOrchestrator(_mediator.Object, _logger.Object))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task Then_Returns_Created_When_The_Log_Is_Created()
    {
        var request = new CreateWorkflowLogRequest
        {
            Sequence = 1,
            SpanId = "12345",
            Stage = "Start",
            Description = "Started import for account 12345",
            Status = "started"
        };

        _mediator.Setup(x => x.Send(It.IsAny<CreateAuditWorkflowLogCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateAuditWorkflowLogCommandResult
            {
                Succeeded = true,
                Message = "Resource created successfully."
            });

        var response = await _controller.CreateWorkflowLog("job-1", "workflow-1", request);

        response.Should().BeOfType<CreatedResult>();
        var created = response as CreatedResult;
        created!.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Location.Should().Be("/jobs/job-1/workflows/workflow-1/logs");
    }

    [Test]
    public async Task Then_Returns_Bad_Request_When_Status_Is_Invalid()
    {
        var response = await _controller.CreateWorkflowLog("job-1", "workflow-1", new CreateWorkflowLogRequest
        {
            Sequence = 1,
            SpanId = "12345",
            Stage = "Start",
            Description = "Invalid status",
            Status = "unknown"
        });

        response.Should().BeOfType<BadRequestObjectResult>();
    }
}
