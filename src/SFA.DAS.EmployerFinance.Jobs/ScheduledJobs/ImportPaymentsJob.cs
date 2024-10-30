using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;

public class ImportPaymentsJob(IMessageSession messageSession)
{
    public Task Run([TimerTrigger("0 0 * * * *")] TimerInfo timer, ILogger logger)
    {
        return messageSession.Send(new ImportPaymentsCommand());
    }
}