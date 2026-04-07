using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobLogs;

public class GetAuditJobLogsQueryResponse
{
    public PagedResult<WorkflowLogDto> Logs { get; set; }
}
