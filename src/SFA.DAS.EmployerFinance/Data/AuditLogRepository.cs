using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Data;

public class AuditLogRepository(EmployerFinanceDbContext dbContext) : IAuditLogRepository
{
    public Task<bool> JobExists(string jobId, CancellationToken cancellationToken = default)
    {
        return dbContext.JobRuns.AnyAsync(x => x.JobId == jobId, cancellationToken);
    }

    public Task<bool> WorkflowLogExists(string workflowId, int sequence, CancellationToken cancellationToken = default)
    {
        return dbContext.WorkflowLogs.AnyAsync(x => x.WorkflowId == workflowId && x.Sequence == sequence, cancellationToken);
    }

    public async Task CreateJob(JobRun jobRun, CancellationToken cancellationToken = default)
    {
        dbContext.JobRuns.Add(jobRun);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateWorkflowLog(WorkflowLog workflowLog, CancellationToken cancellationToken = default)
    {
        dbContext.WorkflowLogs.Add(workflowLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuditJobSummaryDto> GetJobSummary(string jobId, CancellationToken cancellationToken = default)
    {
        var job = await dbContext.JobRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.JobId == jobId, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var latestStatuses = await GetLatestWorkflowStatuses([jobId], cancellationToken);
        return BuildJobSummary(job, latestStatuses);
    }

    public async Task<PagedResult<AuditJobSummaryDto>> GetJobs(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await dbContext.JobRuns.CountAsync(cancellationToken);

        var jobs = await dbContext.JobRuns
            .AsNoTracking()
            .OrderByDescending(x => x.DateStarted)
            .ThenByDescending(x => x.JobId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var latestStatuses = await GetLatestWorkflowStatuses(jobs.Select(x => x.JobId), cancellationToken);

        return new PagedResult<AuditJobSummaryDto>
        {
            Items = jobs.Select(job => BuildJobSummary(job, latestStatuses)).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<WorkflowLogDto>> GetJobLogs(string jobId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.WorkflowLogs
            .AsNoTracking()
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.Created)
            .ThenByDescending(x => x.Sequence);

        return await BuildPagedWorkflowLogs(query, page, pageSize, cancellationToken);
    }

    public async Task<PagedResult<WorkflowLogDto>> GetWorkflowLogs(string jobId, string workflowId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.WorkflowLogs
            .AsNoTracking()
            .Where(x => x.JobId == jobId && x.WorkflowId == workflowId)
            .OrderByDescending(x => x.Sequence)
            .ThenByDescending(x => x.Created);

        return await BuildPagedWorkflowLogs(query, page, pageSize, cancellationToken);
    }

    private static async Task<PagedResult<WorkflowLogDto>> BuildPagedWorkflowLogs(IQueryable<WorkflowLog> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WorkflowLogDto
            {
                JobId = x.JobId,
                WorkflowId = x.WorkflowId,
                SpanId = x.SpanId,
                Sequence = x.Sequence,
                Stage = x.Stage,
                Description = x.Description,
                Status = x.Status,
                DataJson = x.DataJson,
                ErrorCode = x.ErrorCode,
                ErrorMessage = x.ErrorMessage,
                Created = x.Created
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkflowLogDto>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    private async Task<IReadOnlyCollection<LatestWorkflowStatus>> GetLatestWorkflowStatuses(IEnumerable<string> jobIds, CancellationToken cancellationToken)
    {
        var jobIdList = jobIds.Distinct().ToList();
        if (jobIdList.Count == 0)
        {
            return Array.Empty<LatestWorkflowStatus>();
        }

        var latestSequences =
            from log in dbContext.WorkflowLogs
            where jobIdList.Contains(log.JobId)
            group log by new { log.JobId, log.WorkflowId } into workflowGroup
            select new
            {
                workflowGroup.Key.JobId,
                workflowGroup.Key.WorkflowId,
                Sequence = workflowGroup.Max(x => x.Sequence)
            };

        var latestStatuses =
            from log in dbContext.WorkflowLogs
            join latest in latestSequences
                on new { log.JobId, log.WorkflowId, log.Sequence }
                equals new { latest.JobId, latest.WorkflowId, latest.Sequence }
            select new LatestWorkflowStatus(log.JobId, log.Status);

        return await latestStatuses.ToListAsync(cancellationToken);
    }

    private static AuditJobSummaryDto BuildJobSummary(JobRun job, IEnumerable<LatestWorkflowStatus> latestStatuses)
    {
        var statuses = latestStatuses
            .Where(x => x.JobId == job.JobId)
            .Select(x => x.Status)
            .ToList();

        var running = statuses.Count(x => x == WorkflowLogStatuses.Started || x == WorkflowLogStatuses.InProgress);
        var completed = statuses.Count(x => x == WorkflowLogStatuses.Completed);
        var failed = statuses.Count(x => x == WorkflowLogStatuses.Failed);

        return new AuditJobSummaryDto
        {
            Id = job.JobId,
            Description = job.Description,
            DateStarted = job.DateStarted,
            NumRecords = job.NumRecords,
            Running = running,
            Completed = completed,
            Failed = failed,
            Status = CalculateStatus(job.NumRecords, completed, failed)
        };
    }

    private static string CalculateStatus(int numRecords, int completed, int failed)
    {
        if (numRecords > 0 && completed == numRecords && failed == 0)
        {
            return JobRunStatuses.Completed;
        }

        if (numRecords > 0 && completed + failed >= numRecords && failed > 0)
        {
            return JobRunStatuses.Failed;
        }

        return JobRunStatuses.Running;
    }

    private sealed record LatestWorkflowStatus(string JobId, string Status);
}
