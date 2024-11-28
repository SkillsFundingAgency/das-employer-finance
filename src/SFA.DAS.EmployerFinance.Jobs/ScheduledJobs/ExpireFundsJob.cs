using System.Collections.Generic;
using NServiceBus;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;

public class ExpireFundsJob(IMessageSession messageSession, ICurrentDateTime currentDateTime, IEmployerAccountRepository accountRepository)
{
    public async Task Run([TimerTrigger("0 0 0 28 * *")] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation($"Starting {nameof(ExpireFundsJob)}");

        var now = currentDateTime.Now;
        var accounts = await accountRepository.GetAll();

        logger.LogInformation($"Queueing {nameof(ExpireAccountFundsCommand)} messages for {accounts.Count} accounts.");

        var messageTasks = new List<Task>();
        var sendCounter = 0;

        foreach (var account in accounts)
        {
            var sendOptions = new SendOptions();

            sendOptions.SetMessageId($"{nameof(ExpireAccountFundsCommand)}-{now.Year}-{now.Month}-{account.Id}");

            messageTasks.Add(messageSession.Send(new ExpireAccountFundsCommand { AccountId = account.Id }, sendOptions));
            sendCounter++;

            if (sendCounter % 1000 == 0)
            {
                await Task.WhenAll(messageTasks);
                logger.LogInformation($"Queued {sendCounter} of {accounts.Count} messages.");
                messageTasks.Clear();
                await Task.Delay(1000);
            }
        }

        // await final tasks not % 1000
        await Task.WhenAll(messageTasks);

        logger.LogInformation($"{nameof(ExpireFundsJob)} completed.");
    }
}