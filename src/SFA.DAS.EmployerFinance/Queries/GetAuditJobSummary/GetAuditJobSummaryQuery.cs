namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobSummary;

public class GetAuditJobSummaryQuery : IRequest<GetAuditJobSummaryQueryResponse>
{
    public string JobId { get; set; }
}
