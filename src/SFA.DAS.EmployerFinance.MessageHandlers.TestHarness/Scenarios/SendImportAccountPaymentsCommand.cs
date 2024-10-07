using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.MessageHandlers.TestHarness.Scenarios
{
    public class SendImportAccountPaymentsCommand(IMessageSession messageSession)
    {
        public async Task Run()
        {
            var newCommand = new ImportAccountPaymentsCommand { PeriodEndRef = "2324-R11", AccountId = 279 };
            await messageSession.Send(newCommand);
        }
    }
}
