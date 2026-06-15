namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobLogs;

public class GetAuditJobLogsQuery : IRequest<GetAuditJobLogsQueryResponse>
{
    public string JobId { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
