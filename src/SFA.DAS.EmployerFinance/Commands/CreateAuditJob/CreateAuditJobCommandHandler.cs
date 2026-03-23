using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Commands.CreateAuditJob;

public class CreateAuditJobCommandHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<CreateAuditJobCommand, CreateAuditJobCommandResult>
{
    public async Task<CreateAuditJobCommandResult> Handle(CreateAuditJobCommand request, CancellationToken cancellationToken)
    {
        if (await auditLogRepository.JobExists(request.JobId, cancellationToken))
        {
            return new CreateAuditJobCommandResult
            {
                Created = false,
                Message = $"Job '{request.JobId}' already exists."
            };
        }

        await auditLogRepository.CreateJob(new JobRun
        {
            JobId = request.JobId,
            Description = request.Description,
            DateStarted = request.DateStarted,
            NumRecords = request.NumRecords
        }, cancellationToken);

        return new CreateAuditJobCommandResult
        {
            Created = true,
            Message = "Resource created successfully."
        };
    }
}
