using System.Text.Json;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class CreateWorkflowLogRequest
{
    public int Sequence { get; set; }

    public string SpanId { get; set; }

    public string Stage { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public JsonElement? Data { get; set; }

    public AuditErrorDetails Error { get; set; }
}
