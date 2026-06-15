namespace SFA.DAS.EmployerFinance.Commands.CreateAuditWorkflowLog;

public class CreateAuditWorkflowLogCommandResult
{
    public bool Succeeded { get; set; }

    public bool DuplicateIgnored { get; set; }

    public string Message { get; set; }
}
