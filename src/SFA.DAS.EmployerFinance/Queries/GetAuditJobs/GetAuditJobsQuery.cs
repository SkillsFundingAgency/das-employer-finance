namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobs;

public class GetAuditJobsQuery : IRequest<GetAuditJobsQueryResponse>
{
    public int Page { get; set; }

    public int PageSize { get; set; }
}
