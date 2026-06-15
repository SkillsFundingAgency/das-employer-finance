using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobSummary;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AuditLogControllerTests;

public class WhenIGetAuditJob
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
    public async Task Then_Returns_Ok_When_The_Job_Exists()
    {
        _mediator.Setup(x => x.Send(It.IsAny<GetAuditJobSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAuditJobSummaryQueryResponse
            {
                Job = new AuditJobSummaryDto
                {
                    Id = "job-1",
                    Description = "Import Payments",
                    DateStarted = new DateTime(2026, 03, 19, 09, 00, 00, DateTimeKind.Utc),
                    NumRecords = 1250,
                    Running = 47,
                    Completed = 1200,
                    Failed = 3,
                    Status = "running"
                }
            });

        var response = await _controller.GetJob("job-1");

        response.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Then_Returns_NotFound_When_The_Job_Does_Not_Exist()
    {
        _mediator.Setup(x => x.Send(It.IsAny<GetAuditJobSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAuditJobSummaryQueryResponse());

        var response = await _controller.GetJob("missing-job");

        response.Should().BeOfType<NotFoundResult>();
    }
}
