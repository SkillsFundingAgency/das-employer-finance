using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Queries.GetAllEmployerAccounts;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class ProcessPeriodEndPaymentsCommandHandler(
    IMediator mediator,
    ILogger<ProcessPeriodEndPaymentsCommandHandler> logger)
    : IHandleMessages<ProcessPeriodEndPaymentsCommand>
{
    public async Task Handle(ProcessPeriodEndPaymentsCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation($"Creating payment queue message for all accounts for period end ref: '{message.PeriodEndRef}'");
        
        var response = await mediator.Send(new GetAllEmployerAccountsRequest());
        
        await Parallel.ForEachAsync(response.Accounts, new ParallelOptions { MaxDegreeOfParallelism = 100 },
            async (account, _) =>
            {
                logger.LogInformation(
                    $"Creating payment queue message for account ID: '{account.Id}' period end ref: '{message.PeriodEndRef}'");

                var sendOptions = new SendOptions();

                sendOptions.RouteToThisEndpoint();
                // sendOptions.RequireImmediateDispatch(); // Circumvent sender outbox
                sendOptions.SetMessageId(
                    $"{nameof(ImportAccountPaymentsCommand)}-{message.PeriodEndRef}-{account.Id}"); // Allow receiver outbox to de-dupe

                await context
                    .Send(
                        new ImportAccountPaymentsCommand
                            { PeriodEndRef = message.PeriodEndRef, AccountId = account.Id }, sendOptions)
                    .ConfigureAwait(false);
            }).ConfigureAwait(false);

        logger.LogInformation($"Completed payment message queuing for period end ref: '{message.PeriodEndRef}'");
    }
}