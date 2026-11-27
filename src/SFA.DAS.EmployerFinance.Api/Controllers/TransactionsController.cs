using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;
using SFA.DAS.EmployerFinance.Queries.GetLatestTransactionTypeTransactionIdsForAccount;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/transaction-lines")]
public class TransactionsController(TransactionLineStagingOrchestrator orchestrator, ILogger<TransactionsController> logger, IMediator mediator) : ControllerBase
{
    [Route("staging", Name = "BulkTransactionLineStagingIngest")]
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
    
    [HttpGet("{accountId:long}/latest-transaction-type-ids")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [ProducesResponseType(typeof(LatestTransactionTypeTransactionIdsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatestTransactionTypeTransactionIdsForAccount(long accountId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting latest transaction id for each transaction type for accountId {AccountId}", accountId);

        var result = await mediator.Send(new GetLatestTransactionTypeTransactionIdsForAccountQuery(accountId), cancellationToken);

        return Ok(new LatestTransactionTypeTransactionIdsResponse
        {
            LatestDeclarationId = result.LatestDeclarationId,
            LatestPaymentId = result.LatestPaymentId,
            LatestExpiryId = result.LatestExpiryId
        });
    }
}