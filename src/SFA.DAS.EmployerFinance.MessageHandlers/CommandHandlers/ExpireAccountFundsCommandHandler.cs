using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers;

public class ExpireAccountFundsCommandHandler : IHandleMessages<ExpireAccountFundsCommand>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ILevyFundsInRepository _levyFundsInRepository;
    private readonly IPaymentFundsOutRepository _paymentFundsOutRepository;
    private readonly IExpiredFunds _expiredFunds;
    private readonly IExpiredFundsRepository _expiredFundsRepository;
    private readonly ILogger<ExpireAccountFundsCommandHandler> _logger;
    private readonly EmployerFinanceConfiguration _configuration;

    public ExpireAccountFundsCommandHandler(
        ICurrentDateTime currentDateTime,
        ILevyFundsInRepository levyFundsInRepository,
        IPaymentFundsOutRepository paymentFundsOutRepository,
        IExpiredFunds expiredFunds,
        IExpiredFundsRepository expiredFundsRepository,
        ILogger<ExpireAccountFundsCommandHandler> logger,
        EmployerFinanceConfiguration configuration)
    {
        _currentDateTime = currentDateTime;
        _levyFundsInRepository = levyFundsInRepository;
        _paymentFundsOutRepository = paymentFundsOutRepository;
        _expiredFunds = expiredFunds;
        _expiredFundsRepository = expiredFundsRepository;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Handle(ExpireAccountFundsCommand message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Expiring funds for account ID '{AccountId}' with expiry period '{FundsExpiryPeriod}'", message.AccountId, _configuration.FundsExpiryPeriod);

        var now = _currentDateTime.Now;
        var fundsIn = await _levyFundsInRepository.GetLevyFundsIn(message.AccountId);
        var fundsOut = await _paymentFundsOutRepository.GetPaymentFundsOut(message.AccountId);
        var existingExpiredFunds = await _expiredFundsRepository.Get(message.AccountId);

        var expiredFunds = _expiredFunds.GetExpiredFunds(
            fundsIn.ToCalendarPeriodDictionary(),
            fundsOut.ToCalendarPeriodDictionary(),
            existingExpiredFunds.ToCalendarPeriodDictionary(),
            _configuration.FundsExpiryPeriod,
            now);

        var currentCalendarPeriod = new CalendarPeriod(_currentDateTime.Now.Year, _currentDateTime.Now.Month);
        if (!expiredFunds.ContainsKey(currentCalendarPeriod))
        {
            expiredFunds.Add(currentCalendarPeriod, 0);
        }

        await _expiredFundsRepository.Create(message.AccountId, expiredFunds.ToExpiredFundsList(), now);

        //todo: do we publish the event if no fund expired? we could add a bool like the levy declared message
        // once an account has an expired fund, we'll publish every run, even if no additional funds have expired
        if (expiredFunds.Any(ef => ef.Value != 0m))
            await PublishAccountFundsExpiredEvent(context, message.AccountId);

        _logger.LogInformation("Expired '{ExpiredFundsCount}' month(s) of funds for account ID '{AccountId}' with expiry period '{FundsExpiryPeriod}'", expiredFunds.Count, message.AccountId, _configuration.FundsExpiryPeriod);
    }

    private static async Task PublishAccountFundsExpiredEvent(IMessageHandlerContext context, long accountId)
    {
        await context.Publish(new AccountFundsExpiredEvent
        {
            AccountId = accountId,
            Created = DateTime.UtcNow
        });
    }
}