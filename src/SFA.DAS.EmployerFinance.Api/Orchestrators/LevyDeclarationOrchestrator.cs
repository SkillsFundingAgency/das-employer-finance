using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class LevyDeclarationOrchestrator(IMediator mediator, ILogger<LevyDeclarationOrchestrator> logger)
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<LevyDeclarationOrchestrator> _logger = logger;

    public async Task<PersistLevyDeclarationsResponse> PersistLevyDeclarations(PersistLevyDeclarationRequestData request)
    {
        _logger.LogInformation(
            "Persisting levy declarations for AccountId {AccountId}, EmpRef {EmpRef}, count {Count}",
            request.AccountId,
            request.EmpRef,
            request.Declarations?.Count ?? 0);

        return await _mediator.Send(new PersistLevyDeclarationsCommand { Data = request });
    }

    public async Task<List<string>> GetSubmissionIds(string empRef)
    {
        _logger.LogInformation("Requesting levy declaration submission ids for empRef {EmpRef}", empRef);

        var ids = await _mediator.Send(new GetLevyDeclarationSubmissionIdsQuery
        {
            EmpRef = empRef
        });

        _logger.LogInformation("Received {Count} levy declaration submission ids for empRef {EmpRef}", ids.Count, empRef);

        List<string> strIds = ids.ConvertAll(id => id.ToString());
        return strIds;
    }

    public async Task<List<ExistingPeriod12LevyDeclarationResult>> GetExistingPeriod12LevyDeclarations(string empRef)
    {
        _logger.LogInformation("Requesting existing period 12 levy declarations for empRef {EmpRef}", empRef);

        var declarations = await _mediator.Send(new GetExistingPeriod12LevyDeclarationsQuery
        {
            EmpRef = empRef
        });

        _logger.LogInformation(
            "Received {Count} existing period 12 levy declarations for empRef {EmpRef}",
            declarations.Count,
            empRef);

        return declarations;
    }

    public async Task<LastSubmissionDateResult> GetLastSubmissionDate(string empRef)
    {
        _logger.LogInformation("Requesting last levy declaration submission date for empRef {EmpRef}", empRef);

        var existingDeclaration = await _mediator.Send(new GetLastLevyDeclarationQuery
        {
            EmpRef = empRef
        });

        DateTime? dateFrom = null;
        if (existingDeclaration?.Transaction?.SubmissionDate != null &&
            existingDeclaration.Transaction.SubmissionDate != DateTime.MinValue)
        {
            dateFrom = existingDeclaration.Transaction.SubmissionDate.AddDays(-1);
        }

        _logger.LogInformation("Received last levy declaration submission date for empRef {EmpRef}", empRef);

        return new LastSubmissionDateResult
        {
            LastSumissionDate = dateFrom
        };
    }
}
