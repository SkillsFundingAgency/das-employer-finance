using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetAuditJobSummary;

public class GetAuditJobSummaryQueryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditJobSummaryQuery, GetAuditJobSummaryQueryResponse>
{
    public async Task<GetAuditJobSummaryQueryResponse> Handle(GetAuditJobSummaryQuery request, CancellationToken cancellationToken)
    {
        return new GetAuditJobSummaryQueryResponse
        {
            Job = await auditLogRepository.GetJobSummary(request.JobId, cancellationToken)
        };
    }
}
