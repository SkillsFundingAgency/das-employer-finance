using System.Collections.Generic;
using NServiceBus;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;

public class ImportLevyDeclarationsJob(
    IMessageSession messageSession,
    IEmployerAccountRepository accountRepository,
    IPayeRepository payeRepository)
{
    public async Task Run([TimerTrigger("0 0 15 23 * *")] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation("Starting {TypeName}", nameof(ImportLevyDeclarationsJob));
        var employerAccounts = await accountRepository.GetAll();

        logger.LogInformation("Updating {EmployerAccountsCount} levy accounts", employerAccounts.Count);

        var messageTasks = new List<Task>();
        var sendCounter = 0;

        foreach (var account in employerAccounts)
        {
            var schemes = await payeRepository.GetGovernmentGatewayOnlySchemesByEmployerId(account.Id);

            if (schemes?.SchemesList == null)
            {
                continue;
            }

            foreach (var scheme in schemes.SchemesList)
            {
                logger.LogDebug("Creating update levy account message for account {AccountName} (ID: {AccountId}) scheme {SchemeEmpRef}", account.Name, account.Id, scheme.EmpRef);

                messageTasks.Add(messageSession.Send(new ImportAccountLevyDeclarationsCommand(account.Id, scheme.EmpRef)));
            }

            sendCounter++;

            if (sendCounter % 1000 == 0)
            {
                await Task.WhenAll(messageTasks);
                
                logger.LogInformation("Queued {SendCounter} of {Count} accounts.", sendCounter, employerAccounts.Count);
                
                messageTasks.Clear();
                
                await Task.Delay(1000);
            }
        }

        // await final tasks not % 1000
        await Task.WhenAll(messageTasks);

        logger.LogInformation("{TypeName} completed.",nameof(ImportLevyDeclarationsJob));
    }
}