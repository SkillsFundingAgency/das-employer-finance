﻿using System.Collections.Generic;
using Irony.Ast;
using NServiceBus;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.Jobs.ScheduledJobs;

public class ImportLevyDeclarationsJob
{
    private readonly IMessageSession _messageSession;
    private readonly IEmployerAccountRepository _accountRepository;
    private readonly IPayeRepository _payeRepository;

    public ImportLevyDeclarationsJob(
        IMessageSession messageSession,
        IEmployerAccountRepository accountRepository,
        IPayeRepository payeRepository)
    {
        _messageSession = messageSession;
        _accountRepository = accountRepository;
        _payeRepository = payeRepository;
    }

    public async Task Run([TimerTrigger("0 0 15 23 * *")] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation($"Starting {nameof(ImportLevyDeclarationsJob)}");
        var employerAccounts = await _accountRepository.GetAll();

        logger.LogInformation($"Updating {employerAccounts.Count} levy accounts");

        var messageTasks = new List<Task>();
        var sendCounter = 0;

        foreach (var account in employerAccounts)
        {
            var schemes = await _payeRepository.GetGovernmentGatewayOnlySchemesByEmployerId(account.Id);

            if (schemes?.SchemesList == null)
            {
                continue;
            }

            foreach (var scheme in schemes.SchemesList)
            {
                logger.LogDebug($"Creating update levy account message for account {account.Name} (ID: {account.Id}) scheme {scheme.EmpRef}");

                messageTasks.Add(_messageSession.Send(new ImportAccountLevyDeclarationsCommand(account.Id, scheme.EmpRef)));
            }

            sendCounter++;

            if (sendCounter % 1000 == 0)
            {
                await ProcessAll(messageTasks);
                logger.LogInformation($"Queued {sendCounter} of {employerAccounts.Count} accounts.");
                messageTasks.Clear();
                await Task.Delay(1000);
            }
        }

        // await final tasks not % 1000
        await ProcessAll(messageTasks);

        logger.LogInformation($"{nameof(ImportLevyDeclarationsJob)} completed.");
    }

    private Task ProcessAll(List<Task> messageTasks)
    {
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
        return Parallel.ForEachAsync(messageTasks, parallelOptions, async (task, cancellationToken) =>
        {
            await task;
        });
    }
}