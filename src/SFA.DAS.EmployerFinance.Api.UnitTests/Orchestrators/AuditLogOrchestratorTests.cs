using System.Text.Json;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobLogs;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobs;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Orchestrators;

public class AuditLogOrchestratorTests
{
    [Test]
    public async Task Then_CreateWorkflowLog_Maps_Data_And_Error()
    {
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<AuditLogOrchestrator>>();
        var orchestrator = new AuditLogOrchestrator(mediator.Object, logger.Object);

        CreateAuditWorkflowLogCommand capturedCommand = null;
        mediator.Setup(x => x.Send(It.IsAny<CreateAuditWorkflowLogCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<CreateAuditWorkflowLogCommandResult>, CancellationToken>((request, _) => capturedCommand = request as CreateAuditWorkflowLogCommand)
            .ReturnsAsync(new CreateAuditWorkflowLogCommandResult
            {
                Succeeded = true,
                Message = "Resource created successfully."
            });

        await orchestrator.CreateWorkflowLog("job-1", "workflow-1", new CreateWorkflowLogRequest
        {
            Sequence = 1,
            SpanId = "12345",
            Stage = "Start",
            Description = "Started import",
            Status = "started",
            Data = JsonDocument.Parse("{\"accountId\":12345}").RootElement.Clone(),
            Error = new AuditErrorDetails
            {
                Code = "ERR",
                Message = "Failure"
            }
        });

        capturedCommand.Should().NotBeNull();
        capturedCommand!.DataJson.Should().Be("{\"accountId\":12345}");
        capturedCommand.ErrorCode.Should().Be("ERR");
        capturedCommand.ErrorMessage.Should().Be("Failure");
    }

    [Test]
    public async Task Then_GetJobs_Maps_Dto_To_Api_Type()
    {
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<AuditLogOrchestrator>>();
        var orchestrator = new AuditLogOrchestrator(mediator.Object, logger.Object);

        mediator.Setup(x => x.Send(It.IsAny<GetAuditJobsQuery>(), It.IsAny<CancellationToken>()))
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
                            NumRecords = 10,
                            Running = 1,
                            Completed = 8,
                            Failed = 1,
                            Status = "running"
                        }
                    ],
                    TotalCount = 1
                }
            });

        var result = await orchestrator.GetJobs(1, 20);

        result.Items.Should().ContainSingle();
        result.Items.Single().Id.Should().Be("job-1");
    }

    [Test]
    public async Task Then_GetJobLogs_Parses_Json_And_Error()
    {
        var mediator = new Mock<IMediator>();
        var logger = new Mock<ILogger<AuditLogOrchestrator>>();
        var orchestrator = new AuditLogOrchestrator(mediator.Object, logger.Object);

        mediator.Setup(x => x.Send(It.IsAny<GetAuditJobLogsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAuditJobLogsQueryResponse
            {
                Logs = new PagedResult<WorkflowLogDto>
                {
                    Items =
                    [
                        new WorkflowLogDto
                        {
                            JobId = "job-1",
                            WorkflowId = "workflow-1",
                            SpanId = "12345",
                            Sequence = 1,
                            Stage = "Start",
                            Description = "Started import",
                            Status = "failed",
                            DataJson = "{\"accountId\":12345}",
                            ErrorCode = "ERR",
                            ErrorMessage = "Failure",
                            Created = DateTime.UtcNow
                        }
                    ],
                    TotalCount = 1
                }
            });

        var result = await orchestrator.GetJobLogs("job-1", 1, 20);

        result.Items.Should().ContainSingle();
        result.Items.Single().Data!.Value.GetProperty("accountId").GetInt32().Should().Be(12345);
        result.Items.Single().Error!.Code.Should().Be("ERR");
    }
}
