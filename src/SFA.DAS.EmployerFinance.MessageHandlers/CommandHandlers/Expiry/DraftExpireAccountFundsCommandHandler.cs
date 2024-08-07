﻿using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers.Expiry;

public class DraftExpireAccountFundsCommandHandler : IHandleMessages<DraftExpireAccountFundsCommand>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly ILevyFundsInRepository _levyFundsInRepository;
    private readonly IPaymentFundsOutRepository _paymentFundsOutRepository;
    private readonly IExpiredFunds _expiredFunds;
    private readonly IExpiredFundsRepository _expiredFundsRepository;
    private readonly ILogger<DraftExpireAccountFundsCommandHandler> _logger;
    private readonly EmployerFinanceConfiguration _configuration;

    public DraftExpireAccountFundsCommandHandler(
        ICurrentDateTime currentDateTime,
        ILevyFundsInRepository levyFundsInRepository,
        IPaymentFundsOutRepository paymentFundsOutRepository,
        IExpiredFunds expiredFunds,
        IExpiredFundsRepository expiredFundsRepository,
        ILogger<DraftExpireAccountFundsCommandHandler> logger,
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

    public async Task Handle(DraftExpireAccountFundsCommand message, IMessageHandlerContext context)
    {
        _logger.LogInformation($"DRAFT: Expiring funds for account ID '{message.AccountId}' with expiry period '{_configuration.FundsExpiryPeriod}'");

        var now = message.DateTo.HasValue ? new DateTime(message.DateTo.Value.Year, message.DateTo.Value.Month, 28) : _currentDateTime.Now;
        var fundsIn = await _levyFundsInRepository.GetLevyFundsIn(message.AccountId);
        var fundsOut = (await _paymentFundsOutRepository.GetPaymentFundsOut(message.AccountId)).ToList();
        var existingExpiredFunds = await _expiredFundsRepository.Get(message.AccountId);

        if (!existingExpiredFunds.Any())
        {
            existingExpiredFunds = await _expiredFundsRepository.GetDraft(message.AccountId);
        }

        if(message.DateTo != null && fundsOut.Count>0)
        {
            fundsOut = fundsOut.Where(c =>
                new DateTime(c.CalendarPeriodYear, c.CalendarPeriodMonth, 1) <
                new DateTime(message.DateTo.Value.Year, message.DateTo.Value.Month, 1)).ToList();
        }

        var expiredFunds = _expiredFunds.GetExpiredFunds(
            fundsIn.ToCalendarPeriodDictionary(),
            fundsOut.ToCalendarPeriodDictionary(),
            existingExpiredFunds.ToCalendarPeriodDictionary(),
            _configuration.FundsExpiryPeriod,
            now);

        if (!message.DateTo.HasValue)
        {
            message.DateTo = DateTime.UtcNow;
        }

        var currentCalendarPeriod = new CalendarPeriod(message.DateTo.Value.Year, message.DateTo.Value.Month);
        if(!expiredFunds.ContainsKey(currentCalendarPeriod))
        {
            expiredFunds.Add(currentCalendarPeriod,0);
        }
            
        await _expiredFundsRepository.CreateDraft(message.AccountId, expiredFunds.Where(c=>c.Key.Equals(currentCalendarPeriod)).ToDictionary(key=>key.Key, value=>value.Value).ToExpiredFundsList(), now);

        _logger.LogInformation($"DRAFT: Expired '{expiredFunds.Count}' month(s) of funds for account ID '{message.AccountId}' with expiry period '{_configuration.FundsExpiryPeriod}'");
    }
}