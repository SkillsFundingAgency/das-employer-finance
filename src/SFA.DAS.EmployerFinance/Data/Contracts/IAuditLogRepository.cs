using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IAuditLogRepository
{
    Task<bool> JobExists(string jobId, CancellationToken cancellationToken = default);

    Task<bool> WorkflowLogExists(string workflowId, int sequence, CancellationToken cancellationToken = default);

    Task CreateJob(JobRun jobRun, CancellationToken cancellationToken = default);

    Task CreateWorkflowLog(WorkflowLog workflowLog, CancellationToken cancellationToken = default);

    Task<AuditJobSummaryDto> GetJobSummary(string jobId, CancellationToken cancellationToken = default);

    Task<PagedResult<AuditJobSummaryDto>> GetJobs(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkflowLogDto>> GetJobLogs(string jobId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<WorkflowLogDto>> GetWorkflowLogs(string jobId, string workflowId, int page, int pageSize, CancellationToken cancellationToken = default);
}
