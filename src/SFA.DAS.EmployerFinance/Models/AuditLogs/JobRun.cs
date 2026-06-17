namespace SFA.DAS.EmployerFinance.Models.AuditLogs;

public class JobRun : Entity
{
    public string JobId { get; set; }

    public string Description { get; set; }

    public DateTime DateStarted { get; set; }

    public int NumRecords { get; set; }

    public virtual ICollection<WorkflowLog> WorkflowLogs { get; set; } = new List<WorkflowLog>();
}
