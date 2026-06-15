using System.Text.Json;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateAuditJob;
using SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobLogs;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobs;
using SFA.DAS.EmployerFinance.Queries.GetAuditJobSummary;
using SFA.DAS.EmployerFinance.Queries.GetAuditWorkflowLogs;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class AuditLogOrchestrator(IMediator mediator, ILogger<AuditLogOrchestrator> logger)
{
    public async Task<CreateAuditJobCommandResult> CreateJob(CreateAuditJobRequest request)
    {
        logger.LogInformation("Creating audit job {JobId}", request.Id);

        return await mediator.Send(new CreateAuditJobCommand
        {
            JobId = request.Id,
            Description = request.Description?.Trim(),
            DateStarted = request.DateStarted,
            NumRecords = request.NumRecords
        });
    }

    public async Task<CreateAuditWorkflowLogCommandResult> CreateWorkflowLog(string jobId, string workflowId, CreateWorkflowLogRequest request)
    {
        logger.LogInformation("Creating audit workflow log for job {JobId}, workflow {WorkflowId}, sequence {Sequence}", jobId, workflowId, request.Sequence);

        return await mediator.Send(new CreateAuditWorkflowLogCommand
        {
            JobId = jobId,
            WorkflowId = workflowId,
            SpanId = request.SpanId?.Trim(),
            Sequence = request.Sequence,
            Stage = request.Stage?.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            DataJson = GetDataJson(request.Data),
            ErrorCode = request.Error?.Code?.Trim(),
            ErrorMessage = request.Error?.Message?.Trim()
        });
    }

    public async Task<PagedResponse<AuditJobSummary>> GetJobs(int page, int pageSize)
    {
        var response = await mediator.Send(new GetAuditJobsQuery
        {
            Page = page,
            PageSize = pageSize
        });

        return new PagedResponse<AuditJobSummary>
        {
            Items = response.Jobs.Items.Select(MapJob).ToList(),
            TotalCount = response.Jobs.TotalCount
        };
    }

    public async Task<AuditJobSummary> GetJob(string jobId)
    {
        var response = await mediator.Send(new GetAuditJobSummaryQuery
        {
            JobId = jobId
        });

        return response.Job == null ? null : MapJob(response.Job);
    }

    public async Task<PagedResponse<WorkflowLogEntry>> GetJobLogs(string jobId, int page, int pageSize)
    {
        var response = await mediator.Send(new GetAuditJobLogsQuery
        {
            JobId = jobId,
            Page = page,
            PageSize = pageSize
        });

        return new PagedResponse<WorkflowLogEntry>
        {
            Items = response.Logs.Items.Select(MapLog).ToList(),
            TotalCount = response.Logs.TotalCount
        };
    }

    public async Task<PagedResponse<WorkflowLogEntry>> GetWorkflowLogs(string jobId, string workflowId, int page, int pageSize)
    {
        var response = await mediator.Send(new GetAuditWorkflowLogsQuery
        {
            JobId = jobId,
            WorkflowId = workflowId,
            Page = page,
            PageSize = pageSize
        });

        return new PagedResponse<WorkflowLogEntry>
        {
            Items = response.Logs.Items.Select(MapLog).ToList(),
            TotalCount = response.Logs.TotalCount
        };
    }

    private static AuditJobSummary MapJob(AuditJobSummaryDto source)
    {
        return new AuditJobSummary
        {
            Id = source.Id,
            Description = source.Description,
            DateStarted = source.DateStarted,
            NumRecords = source.NumRecords,
            Running = source.Running,
            Completed = source.Completed,
            Failed = source.Failed,
            Status = source.Status
        };
    }

    private static WorkflowLogEntry MapLog(WorkflowLogDto source)
    {
        return new WorkflowLogEntry
        {
            JobId = source.JobId,
            WorkflowId = source.WorkflowId,
            SpanId = source.SpanId,
            Sequence = source.Sequence,
            Stage = source.Stage,
            Description = source.Description,
            Status = source.Status,
            Data = ParseJson(source.DataJson),
            Error = string.IsNullOrWhiteSpace(source.ErrorCode) && string.IsNullOrWhiteSpace(source.ErrorMessage)
                ? null
                : new AuditErrorDetails
                {
                    Code = source.ErrorCode,
                    Message = source.ErrorMessage
                },
            Created = source.Created
        };
    }

    private static string GetDataJson(JsonElement? data)
    {
        if (!data.HasValue || data.Value.ValueKind == JsonValueKind.Undefined || data.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return data.Value.GetRawText();
    }

    private static JsonElement? ParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
