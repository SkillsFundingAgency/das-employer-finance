using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.AuditLogs;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("jobs")]
public class AuditLogController(AuditLogOrchestrator auditLogOrchestrator) : ControllerBase
{
    private const string InvalidRequestCode = "invalid_request";
    private const string InvalidPagingCode = "invalid_paging";

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateAuditJobRequest request)
    {
        if (request == null)
        {
            return BadRequest(CreateError(InvalidRequestCode, "A job payload is required."));
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(CreateError(InvalidRequestCode, "The job payload is invalid."));
        }

        if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Description) || request.DateStarted == default || request.NumRecords < 0)
        {
            return BadRequest(CreateError(InvalidRequestCode, "The job payload is invalid."));
        }

        var result = await auditLogOrchestrator.CreateJob(request);
        if (!result.Created)
        {
            return BadRequest(CreateError("job_exists", result.Message));
        }

        return Created($"/jobs/{request.Id}", new CreateResourceResponse
        {
            Id = request.Id,
            Message = result.Message
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] int page = 1, [FromQuery] int pagesize = 20)
    {
        var validationError = ValidatePaging(page, pagesize);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var result = await auditLogOrchestrator.GetJobs(page, pagesize);
        AddPaginationLinks(page, pagesize, result.TotalCount);

        return Ok(result.Items);
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetJob(string jobId)
    {
        var result = await auditLogOrchestrator.GetJob(jobId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{jobId}/logs")]
    public async Task<IActionResult> GetJobLogs(string jobId, [FromQuery] int page = 1, [FromQuery] int pagesize = 20)
    {
        var validationError = ValidatePaging(page, pagesize);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var result = await auditLogOrchestrator.GetJobLogs(jobId, page, pagesize);
        AddPaginationLinks(page, pagesize, result.TotalCount);

        return Ok(result.Items);
    }

    [HttpPost("{jobId}/workflows/{workflowId}/logs")]
    public async Task<IActionResult> CreateWorkflowLog(string jobId, string workflowId, [FromBody] CreateWorkflowLogRequest request)
    {
        if (request == null)
        {
            return BadRequest(CreateError(InvalidRequestCode, "A workflow log payload is required."));
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(CreateError(InvalidRequestCode, "The workflow log payload is invalid."));
        }

        if (!IsValidWorkflowLogRequest(request, out var message))
        {
            return BadRequest(CreateError(InvalidRequestCode, message));
        }

        var result = await auditLogOrchestrator.CreateWorkflowLog(jobId, workflowId, request);
        if (!result.Succeeded)
        {
            return BadRequest(CreateError(InvalidRequestCode, result.Message));
        }

        return Created($"/jobs/{jobId}/workflows/{workflowId}/logs", new CreateResourceResponse
        {
            Id = $"{workflowId}:{request.Sequence}",
            Message = result.Message
        });
    }

    [HttpGet("{jobId}/workflows/{workflowId}/logs")]
    public async Task<IActionResult> GetWorkflowLogs(string jobId, string workflowId, [FromQuery] int page = 1, [FromQuery] int pagesize = 20)
    {
        var validationError = ValidatePaging(page, pagesize);
        if (validationError != null)
        {
            return BadRequest(validationError);
        }

        var result = await auditLogOrchestrator.GetWorkflowLogs(jobId, workflowId, page, pagesize);
        AddPaginationLinks(page, pagesize, result.TotalCount);

        return Ok(result.Items);
    }

    private static ApiError CreateError(string code, string message)
    {
        return new ApiError
        {
            Code = code,
            Message = message
        };
    }

    private static ApiError ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            return CreateError(InvalidPagingCode, "The page query parameter must be 1 or greater.");
        }

        if (pageSize < 10 || pageSize > 100)
        {
            return CreateError(InvalidPagingCode, "The pagesize query parameter must be between 10 and 100.");
        }

        return null;
    }

    private static bool IsValidWorkflowLogRequest(CreateWorkflowLogRequest request, out string message)
    {
        if (request.Sequence < 1)
        {
            message = "The sequence must be 1 or greater.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.SpanId) ||
            string.IsNullOrWhiteSpace(request.Stage) ||
            string.IsNullOrWhiteSpace(request.Description) ||
            string.IsNullOrWhiteSpace(request.Status))
        {
            message = "The workflow log payload is invalid.";
            return false;
        }

        if (!TryNormalizeStatus(request.Status, out var normalizedStatus))
        {
            message = $"The status '{request.Status}' is not valid.";
            return false;
        }

        request.Status = normalizedStatus;

        if (request.Data.HasValue &&
            request.Data.Value.ValueKind != JsonValueKind.Object &&
            request.Data.Value.ValueKind != JsonValueKind.Null &&
            request.Data.Value.ValueKind != JsonValueKind.Undefined)
        {
            message = "The data field must be a JSON object.";
            return false;
        }

        message = null;
        return true;
    }

    private static bool TryNormalizeStatus(string status, out string normalizedStatus)
    {
        normalizedStatus = WorkflowLogStatuses.ValidStatuses
            .FirstOrDefault(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));

        return !string.IsNullOrWhiteSpace(normalizedStatus);
    }

    private void AddPaginationLinks(int page, int pageSize, int totalCount)
    {
        var links = new List<string>();
        if (page > 1)
        {
            links.Add(BuildLink(page - 1, pageSize, "prev"));
        }

        if (page * pageSize < totalCount)
        {
            links.Add(BuildLink(page + 1, pageSize, "next"));
        }

        if (links.Count > 0)
        {
            Response.Headers.Link = string.Join(", ", links);
        }
    }

    private string BuildLink(int page, int pageSize, string rel)
    {
        var query = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
        query["page"] = page.ToString();
        query["pagesize"] = pageSize.ToString();

        var absolutePath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
        var url = QueryHelpers.AddQueryString(absolutePath, query);
        return $"<{url}>; rel=\"{rel}\"";
    }
}
