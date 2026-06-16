namespace SFA.DAS.EmployerFinance.Models.AuditLogs;

public static class WorkflowLogStatuses
{
    public const string Started = "started";
    public const string InProgress = "inProgress";
    public const string Completed = "completed";
    public const string Failed = "failed";

    public static readonly IReadOnlyCollection<string> ValidStatuses =
    [
        Started,
        InProgress,
        Completed,
        Failed
    ];
}
