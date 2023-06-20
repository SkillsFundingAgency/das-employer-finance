using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Events.ProcessPayment;

public class ProcessPaymentEventHandler : INotificationHandler<ProcessPaymentEvent>
{
    private readonly IDasLevyRepository _dasLevyRepository;
    private readonly ILogger<ProcessPaymentEventHandler> _logger;

    public ProcessPaymentEventHandler(IDasLevyRepository dasLevyRepository, ILogger<ProcessPaymentEventHandler> logger)
    {
        _dasLevyRepository = dasLevyRepository;
        _logger = logger;
    }

    public async Task Handle(ProcessPaymentEvent notification, CancellationToken cancellationToken)
    {
        await _dasLevyRepository.ProcessPaymentData(notification.AccountId);

        _logger.LogInformation("Process Payments Called");
    }
}