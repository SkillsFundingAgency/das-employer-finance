using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class HealthCheckEventHandler : IHandleMessages<HealthCheckEvent>
{
    private readonly EmployerFinanceDbContext _db;

    public HealthCheckEventHandler(EmployerFinanceDbContext db)
    {
        _db = db;
    }

    public async Task Handle(HealthCheckEvent message, IMessageHandlerContext context)
    {
        var healthCheck = await _db.HealthChecks.SingleAsync(h => h.Id == message.Id);

        healthCheck.ReceiveEvent(message);
    }
}