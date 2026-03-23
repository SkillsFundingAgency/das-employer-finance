using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateAuditJob;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AuditLogControllerTests;

public class WhenICreateAuditJob
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
    public async Task Then_Returns_Created_When_The_Job_Is_Created()
    {
        var request = new CreateAuditJobRequest
        {
            Id = "job-1",
            Description = "Import Payments",
            DateStarted = new DateTime(2026, 03, 19, 09, 00, 00, DateTimeKind.Utc),
            NumRecords = 1250
        };

        _mediator.Setup(x => x.Send(It.IsAny<CreateAuditJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateAuditJobCommandResult
            {
                Created = true,
                Message = "Resource created successfully."
            });

        var response = await _controller.CreateJob(request);

        response.Should().BeOfType<CreatedResult>();
        var created = response as CreatedResult;
        created!.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Location.Should().Be("/jobs/job-1");
    }

    [Test]
    public async Task Then_Returns_Bad_Request_When_The_Request_Is_Invalid()
    {
        var response = await _controller.CreateJob(new CreateAuditJobRequest());

        response.Should().BeOfType<BadRequestObjectResult>();
    }
}
