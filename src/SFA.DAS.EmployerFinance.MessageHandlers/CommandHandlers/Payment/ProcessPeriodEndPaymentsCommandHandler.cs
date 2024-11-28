using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Queries.GetAllEmployerAccounts;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Payment;

public class ProcessPeriodEndPaymentsCommandHandler(
    IMediator mediator,
    ILogger<ProcessPeriodEndPaymentsCommandHandler> logger)
    : IHandleMessages<ProcessPeriodEndPaymentsCommand>
{
    private const int BatchSize = 10000;

    public async Task Handle(ProcessPeriodEndPaymentsCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation("Processing payment queue messages for period end ref: '{PeriodEndRef}', batch: {BatchNumber}", message.PeriodEndRef, message.BatchNumber);

        var getAllEmployerAccountsResponse = await mediator.Send(new GetAllEmployerAccountsRequest());

        var accounts = getAllEmployerAccountsResponse.Accounts
            .OrderBy(a => a.Id)
            .Skip(message.BatchNumber * BatchSize).Take(BatchSize)
            .ToList();

        if (accounts.Any())
        {
            await ProcessPeriodEndPayments(message, context, accounts);

            await EnqueueNextBatch(message, context);
        }
        else
        {
            logger.LogInformation("No more accounts to process for period end ref: '{PeriodEndRef}'.", message.PeriodEndRef);
        }
    }

    private static async Task EnqueueNextBatch(ProcessPeriodEndPaymentsCommand message, IMessageHandlerContext context)
    {
        var nextBatchCommand = new ProcessPeriodEndPaymentsCommand
        {
            PeriodEndRef = message.PeriodEndRef,
            BatchNumber = message.BatchNumber + 1
        };

        var nextBatchSendOptions = new SendOptions();
        nextBatchSendOptions.RouteToThisEndpoint();
            
        await context.Send(nextBatchCommand, nextBatchSendOptions).ConfigureAwait(false);
    }

    private async Task ProcessPeriodEndPayments(ProcessPeriodEndPaymentsCommand message, IMessageHandlerContext context, List<Account> accounts)
    {
        await Parallel.ForEachAsync(accounts,
            async (account, _) =>
            {
                await ProcessPaymentsForAccount(message, context, account);
            }).ConfigureAwait(false);

        logger.LogInformation("Completed payment message queuing for batch: {BatchNumber} period end ref: '{PeriodEndRef}'", message.BatchNumber, message.PeriodEndRef);
    }

    private async Task ProcessPaymentsForAccount(ProcessPeriodEndPaymentsCommand message, IMessageHandlerContext context, Account account)
    {
        logger.LogInformation("Creating payment queue message for account ID: '{AccountId}' period end ref: '{PeriodEndRef}'", account.Id, message.PeriodEndRef);

        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        sendOptions.RequireImmediateDispatch();
        sendOptions.SetMessageId($"{nameof(ImportAccountPaymentsCommand)}-{message.PeriodEndRef}-{account.Id}");

        var importAccountPaymentsCommand = new ImportAccountPaymentsCommand
        {
            PeriodEndRef = message.PeriodEndRef,
            AccountId = account.Id
        };

        await context.Send(importAccountPaymentsCommand, sendOptions).ConfigureAwait(false);
    }
}