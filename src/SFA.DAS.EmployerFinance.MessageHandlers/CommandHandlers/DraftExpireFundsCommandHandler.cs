using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class DraftExpireFundsCommandHandler : IHandleMessages<DraftExpireFundsCommand>
{
    private readonly IEmployerAccountRepository _accountRepository;
    private readonly ILogger<DraftExpireFundsCommandHandler> _logger;

    public DraftExpireFundsCommandHandler(IEmployerAccountRepository accountRepository, ILogger<DraftExpireFundsCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task Handle(DraftExpireFundsCommand message, IMessageHandlerContext context)
    {
        try
        {
            var accounts = await _accountRepository.GetAll();

            var messageTasks = new List<Task>();
            var sendCounter = 0;

            _logger.LogInformation("Queueing {CommandName} messages for {AccountsCount} accounts.", nameof(DraftExpireAccountFundsCommand), accounts.Count);

            foreach (var account in accounts)
            {
                var sendOptions = new SendOptions();
                sendOptions.RouteToThisEndpoint();
                sendOptions.RequireImmediateDispatch();

                messageTasks.Add(context.Send(new DraftExpireAccountFundsCommand { AccountId = account.Id, DateTo = message.DateTo }, sendOptions));
                sendCounter++;

                if (sendCounter % 1000 == 0)
                {
                    await Task.WhenAll(messageTasks).ConfigureAwait(false);
                    _logger.LogInformation("Queued {SendCounter} of {AccountsCount} messages.", sendCounter, accounts.Count);
                    messageTasks.Clear();
                    await Task.Delay(500).ConfigureAwait(false);
                }
            }

            // await final tasks not % 1000
            await Task.WhenAll(messageTasks).ConfigureAwait(false);
            _logger.LogInformation("Queued {SendCounter} of {AccountsCount} messages.", sendCounter, accounts.Count);

            _logger.LogInformation("{CommandName} completed.", nameof(DraftExpireFundsCommandHandler));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{CommandName} failed", nameof(DraftExpireFundsCommandHandler));
            throw;
        }
    }
}