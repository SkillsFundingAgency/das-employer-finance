using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.Services;

// This is a hack to ensure we can create a LevyAddedToAccount with Event as a base class, the class is only published in this project and never consumed here
// The idea is to create an identical class in the Messages project which has no dependencies on Event and SFA.DAS.NServiceBus. But the namespace must match.  
namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public static class LevyAddedToAccountWithEventBaseClass
    {
        public static async Task Publish(IEventPublisher eventPublisher, decimal levyTotalTransactionValue, long accountId)
        {
            await eventPublisher.Publish(new LevyAddedToAccount
            {
                AccountId = accountId,
                Amount = levyTotalTransactionValue
            });
        }
    }

    public class LevyAddedToAccount : Event
    {
        public long AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}