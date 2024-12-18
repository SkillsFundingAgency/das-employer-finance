using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.Services;

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