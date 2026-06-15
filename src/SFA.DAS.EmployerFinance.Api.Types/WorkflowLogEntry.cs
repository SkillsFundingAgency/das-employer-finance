using System.Text.Json;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class WorkflowLogEntry
{
    public string JobId { get; set; }

    public string WorkflowId { get; set; }

    public string SpanId { get; set; }

    public int Sequence { get; set; }

    public string Stage { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public JsonElement? Data { get; set; }

    public AuditErrorDetails Error { get; set; }

    public DateTime Created { get; set; }
}
