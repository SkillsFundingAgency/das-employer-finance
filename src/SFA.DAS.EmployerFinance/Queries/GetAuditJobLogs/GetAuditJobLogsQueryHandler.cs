using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobLogs;

public class GetAuditJobLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditJobLogsQuery, GetAuditJobLogsQueryResponse>
{
    public async Task<GetAuditJobLogsQueryResponse> Handle(GetAuditJobLogsQuery request, CancellationToken cancellationToken)
    {
        return new GetAuditJobLogsQueryResponse
        {
            Logs = await auditLogRepository.GetJobLogs(request.JobId, request.Page, request.PageSize, cancellationToken)
        };
    }
}
