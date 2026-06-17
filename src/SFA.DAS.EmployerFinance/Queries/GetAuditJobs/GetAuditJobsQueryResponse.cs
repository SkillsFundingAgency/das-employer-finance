using SFA.DAS.EmployerFinance.Dtos;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobs;

public class GetAuditJobsQueryResponse
{
    public PagedResult<AuditJobSummaryDto> Jobs { get; set; }
}
