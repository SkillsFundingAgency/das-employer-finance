using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditWorkflowLogs;

public class GetAuditWorkflowLogsQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditWorkflowLogsQuery, GetAuditWorkflowLogsQueryResponse>
{
    public async Task<GetAuditWorkflowLogsQueryResponse> Handle(GetAuditWorkflowLogsQuery request, CancellationToken cancellationToken)
    {
        return new GetAuditWorkflowLogsQueryResponse
        {
            Logs = await auditLogRepository.GetWorkflowLogs(request.JobId, request.WorkflowId, request.Page, request.PageSize, cancellationToken)
        };
    }
}
