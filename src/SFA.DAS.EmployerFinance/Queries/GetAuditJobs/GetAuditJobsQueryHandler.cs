using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobs;

public class GetAuditJobsQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditJobsQuery, GetAuditJobsQueryResponse>
{
    public async Task<GetAuditJobsQueryResponse> Handle(GetAuditJobsQuery request, CancellationToken cancellationToken)
    {
        return new GetAuditJobsQueryResponse
        {
            Jobs = await auditLogRepository.GetJobs(request.Page, request.PageSize, cancellationToken)
        };
    }
}
