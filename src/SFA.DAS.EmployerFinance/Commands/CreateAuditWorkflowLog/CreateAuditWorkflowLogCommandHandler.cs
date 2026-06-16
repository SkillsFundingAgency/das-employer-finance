using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;

public class CreateAuditWorkflowLogCommandHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<CreateAuditWorkflowLogCommand, CreateAuditWorkflowLogCommandResult>
{
    public async Task<CreateAuditWorkflowLogCommandResult> Handle(CreateAuditWorkflowLogCommand request, CancellationToken cancellationToken)
    {
        if (!await auditLogRepository.JobExists(request.JobId, cancellationToken))
        {
            return new CreateAuditWorkflowLogCommandResult
            {
                Succeeded = false,
                Message = $"Job '{request.JobId}' does not exist."
            };
        }

        if (await auditLogRepository.WorkflowLogExists(request.WorkflowId, request.Sequence, cancellationToken))
        {
            return new CreateAuditWorkflowLogCommandResult
            {
                Succeeded = true,
                DuplicateIgnored = true,
                Message = "Duplicate sequence ignored."
            };
        }

        await auditLogRepository.CreateWorkflowLog(new WorkflowLog
        {
            JobId = request.JobId,
            WorkflowId = request.WorkflowId,
            SpanId = request.SpanId,
            Sequence = request.Sequence,
            Stage = request.Stage,
            Description = request.Description,
            Status = request.Status,
            DataJson = request.DataJson,
            ErrorCode = request.ErrorCode,
            ErrorMessage = request.ErrorMessage,
            Created = DateTime.UtcNow
        }, cancellationToken);

        return new CreateAuditWorkflowLogCommandResult
        {
            Succeeded = true,
            Message = "Resource created successfully."
        };
    }
}
