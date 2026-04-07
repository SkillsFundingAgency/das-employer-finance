namespace SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;

public class CreateAuditWorkflowLogCommand : IRequest<CreateAuditWorkflowLogCommandResult>
{
    public string JobId { get; set; }

    public string WorkflowId { get; set; }

    public string SpanId { get; set; }

    public int Sequence { get; set; }

    public string Stage { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public string DataJson { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorMessage { get; set; }
}
