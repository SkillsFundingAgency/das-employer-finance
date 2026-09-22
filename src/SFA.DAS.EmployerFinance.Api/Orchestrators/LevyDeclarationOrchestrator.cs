using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;
using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class LevyDeclarationOrchestrator(IMediator mediator, ILogger<LevyDeclarationOrchestrator> logger)
{
    public async Task<PersistLevyDeclarationsResponse> PersistLevyDeclarations(PersistLevyDeclarationRequestData request)
    {
        logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Persisting levy declarations for AccountId {AccountId}, EmpRef {EmpRef}, count {Count}, GenerateTransactions {GenerateTransactions}",
            request.CorrelationId,
            request.AccountId,
            request.EmpRef,
            request.Declarations?.Count ?? 0,
            request.GenerateTransactions);

        return await mediator.Send(new PersistLevyDeclarationsCommand { Data = request });
    }

    public async Task<List<string>> GetSubmissionIds(string empRef)
    {
        logger.LogInformation("Requesting levy declaration submission ids for empRef {EmpRef}", empRef);

        var ids = await mediator.Send(new GetLevyDeclarationSubmissionIdsQuery
        {
            EmpRef = empRef
        });

        logger.LogInformation("Received {Count} levy declaration submission ids for empRef {EmpRef}", ids.Count, empRef);

        List<string> strIds = ids.ConvertAll(id => id.ToString());
        return strIds;
    }

    public async Task<List<ExistingPeriod12LevyDeclarationResult>> GetExistingPeriod12LevyDeclarations(string empRef)
    {
        logger.LogInformation("Requesting existing period 12 levy declarations for empRef {EmpRef}", empRef);

        var declarations = await mediator.Send(new GetExistingPeriod12LevyDeclarationsQuery
        {
            EmpRef = empRef
        });

        logger.LogInformation(
            "Received {Count} existing period 12 levy declarations for empRef {EmpRef}",
            declarations.Count,
            empRef);

        return declarations;
    }

    public async Task<LastSubmissionDateResult> GetLastSubmissionDate(string empRef)
    {
        logger.LogInformation("Requesting last levy declaration submission date for empRef {EmpRef}", empRef);

        var existingDeclaration = await mediator.Send(new GetLastLevyDeclarationQuery
        {
            EmpRef = empRef
        });

        DateTime? dateFrom = null;
        if (existingDeclaration?.Transaction?.SubmissionDate != null &&
            existingDeclaration.Transaction.SubmissionDate != DateTime.MinValue)
        {
            dateFrom = existingDeclaration.Transaction.SubmissionDate.AddDays(-1);
        }

        logger.LogInformation("Received last levy declaration submission date for empRef {EmpRef}", empRef);

        return new LastSubmissionDateResult
        {
            LastSumissionDate = dateFrom
        };
    }

    public async Task<LevySummary> GetLevySummaryByAccountId(long accountId)
    {
        logger.LogInformation("Requesting GetLevySummaryByAccountId for the AccountId {AccountId}", accountId);

        var response = await mediator.Send(new GetLevySummaryByAccountIdQuery(accountId));

        return response.Summary;
    }
}