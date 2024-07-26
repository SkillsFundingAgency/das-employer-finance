using SFA.DAS.EmployerFinance.Messages.Commands;
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
        logger.LogInformation($"Processing payment queue messages for period end ref: '{message.PeriodEndRef}', batch: {message.BatchNumber}");

        var response = await mediator.Send(new GetAllEmployerAccountsRequest());

        var accounts = response.Accounts
            .OrderBy(a => a.Id)
            .Skip(message.BatchNumber * BatchSize).Take(BatchSize)
            .ToList();

        if (accounts.Any())
        {
            await Parallel.ForEachAsync(accounts,
                async (account, _) =>
                {
                    logger.LogInformation(
                        $"Creating payment queue message for account ID: '{account.Id}' period end ref: '{message.PeriodEndRef}'");

                    var sendOptions = new SendOptions();
                    sendOptions.RouteToThisEndpoint();
                    sendOptions.RequireImmediateDispatch();
                    sendOptions.SetMessageId(
                        $"{nameof(ImportAccountPaymentsCommand)}-{message.PeriodEndRef}-{account.Id}");

                    await context
                        .Send(
                            new ImportAccountPaymentsCommand
                                { PeriodEndRef = message.PeriodEndRef, AccountId = account.Id }, sendOptions)
                        .ConfigureAwait(false);
                }).ConfigureAwait(false);

            logger.LogInformation($"Completed payment message queuing for batch: {message.BatchNumber} period end ref: '{message.PeriodEndRef}'");

            var nextBatchCommand = new ProcessPeriodEndPaymentsCommand
            {
                PeriodEndRef = message.PeriodEndRef,
                BatchNumber = message.BatchNumber + 1
            };

            var nextBatchSendOptions = new SendOptions();
            nextBatchSendOptions.RouteToThisEndpoint();
            await context.Send(nextBatchCommand, nextBatchSendOptions).ConfigureAwait(false);
        }
        else
        {
            logger.LogInformation($"No more accounts to process for period end ref: '{message.PeriodEndRef}'.");
        }
    }
}
