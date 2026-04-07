using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobs;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AuditLogControllerTests;

public class WhenIGetAuditJobs
{
    private AuditLogController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<AuditLogOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<AuditLogOrchestrator>>();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("api.test");
        httpContext.Request.Path = "/jobs";

        _controller = new AuditLogController(new AuditLogOrchestrator(_mediator.Object, _logger.Object))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [Test]
    public async Task Then_Returns_Bad_Request_For_Invalid_Paging()
    {
        var response = await _controller.GetJobs(0, 20);

        response.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Then_Adds_Link_Header_When_There_Is_A_Next_Page()
    {
        _mediator.Setup(x => x.Send(It.IsAny<GetAuditJobsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAuditJobsQueryResponse
            {
                Jobs = new PagedResult<AuditJobSummaryDto>
                {
                    Items =
                    [
                        new AuditJobSummaryDto
                        {
                            Id = "job-1",
                            Description = "Import Payments",
                            DateStarted = DateTime.UtcNow,
                            NumRecords = 2,
                            Status = "running"
                        }
                    ],
                    TotalCount = 25
                }
            });

        var response = await _controller.GetJobs(1, 10);

        response.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.Link.ToString().Should().Contain("rel=\"next\"");
    }
}
