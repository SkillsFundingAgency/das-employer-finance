using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditWorkflowLogs;

public class GetAuditWorkflowLogsQueryResponse
{
    public PagedResult<WorkflowLogDto> Logs { get; set; }
}
