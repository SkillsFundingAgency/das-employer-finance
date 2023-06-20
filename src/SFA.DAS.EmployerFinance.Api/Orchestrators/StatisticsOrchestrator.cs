using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetTotalPayments;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class StatisticsOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<StatisticsOrchestrator> _logger;

    public StatisticsOrchestrator(IMediator mediator, ILogger<StatisticsOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public virtual async Task<TotalPaymentsModel> Get()
    {
        _logger.LogInformation($"Requesting Finance statistics");

        var financialStatisticsQueryTask = _mediator.Send(new GetTotalPaymentsQuery());

        return new TotalPaymentsModel
        {            
            TotalPayments = (await financialStatisticsQueryTask).TotalPayments
        };
    }
}