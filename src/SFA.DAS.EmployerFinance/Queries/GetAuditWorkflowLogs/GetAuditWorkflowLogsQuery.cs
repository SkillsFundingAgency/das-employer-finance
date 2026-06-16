namespace SFA.DAS.EmployerFinance.Queries.GetAuditWorkflowLogs;

public class GetAuditWorkflowLogsQuery : IRequest<GetAuditWorkflowLogsQueryResponse>
{
    public string JobId { get; set; }

    public string WorkflowId { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
