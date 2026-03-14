using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;
using SFA.DAS.EmployerFinance.Models.Levy;
using System.Linq;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class EnglishFractionsOrchestrator(IMediator mediator, ILogger<EnglishFractionsOrchestrator> logger)
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<EnglishFractionsOrchestrator> _logger = logger;

    public async Task<EnglishFractionsResponse> PersistEnglishFractions(EnglishFractionsRequest request)
    {
        _logger.LogInformation("Persisting english fractions for empRef {EmpRef}", request.EmpRef);

        var fractions = request.Fractions?
            .Select(f => new DasEnglishFraction
            {
                EmpRef = request.EmpRef,
                DateCalculated = f.DateCalculated,
                Amount = f.Amount
            })
            .ToList() ?? new List<DasEnglishFraction>();

        var response = await _mediator.Send(new PersistEnglishFractionsCommand
        {
            EmployerReference = request.EmpRef,
            UpdateRequired = request.UpdateRequired,
            DateCalculated = request.DateCalculated,
            Fractions = fractions
        });

        return new EnglishFractionsResponse
        {
            Stored = response.Stored,
            Ignored = response.Ignored
        };
    }
}
