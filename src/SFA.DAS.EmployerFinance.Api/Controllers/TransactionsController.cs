using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/transactions")]
public class TransactionsController(TransactionLineStagingOrchestrator orchestrator) : ControllerBase
{
    [Route("/payments/staging", Name = "BulkTransactionLineStagingIngest")]
    [HttpPost]
    public async Task<IActionResult> BulkTransactionLineStagingIngest([FromBody] TransactionLineStagingRequest request)
    {
        if (request?.TransactionLines == null || request.TransactionLines.Count == 0)
            return BadRequest("TransactionLines payload is required.");

        var response = await orchestrator.IngestTransactionLines(request);

        if (response.HasValidationErrors)
            return BadRequest(new { response.IsSuccess, response.Message, validationErrors = response.ValidationErrors });

        if (!response.IsSuccess)
            return StatusCode(StatusCodes.Status500InternalServerError, new { response.IsSuccess, response.Message });

        return StatusCode(StatusCodes.Status201Created, new
        {
            response.IsSuccess,
            response.Message,
            insertedCount = response.InsertedCount
        });
    }
}