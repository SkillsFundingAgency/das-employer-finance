namespace SFA.DAS.EmployerFinance.Models.AuditLogs;

public class WorkflowLog : Entity
{
    public long Id { get; set; }

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

    public DateTime Created { get; set; }

    public virtual JobRun JobRun { get; set; }
}
