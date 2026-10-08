using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.ExpiredFunds;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.ExpireAccountFunds;

public class ExpireAccountFundsCommandHandler(
    IValidator<ExpireAccountFundsCommand> validator,
    ICurrentDateTime currentDateTime,
    ILevyFundsInRepository levyFundsInRepository,
    IPaymentFundsOutRepository paymentFundsOutRepository,
    IExpiredFunds expiredFunds,
    IExpiredFundsRepository expiredFundsRepository,
    EmployerFinanceConfiguration configuration,
    ILogger<ExpireAccountFundsCommandHandler> logger)
    : IRequestHandler<ExpireAccountFundsCommand, ExpireFundsResponse>
{
    public async Task<ExpireFundsResponse> Handle(
        ExpireAccountFundsCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(
                validationResult.ConvertToDataAnnotationsValidationResult(),
                null,
                null);
        }

        logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Expiring funds for AccountId {AccountId} with expiry period {FundsExpiryPeriod}.",
            request.CorrelationId,
            request.AccountId,
            configuration.FundsExpiryPeriod);

        try
        {
            var now = currentDateTime.Now;
            var fundsIn = await levyFundsInRepository.GetLevyFundsIn(request.AccountId);
            var fundsOut = await paymentFundsOutRepository.GetPaymentFundsOut(request.AccountId);
            var existingExpiredFunds = (await expiredFundsRepository.Get(request.AccountId)).ToList();

            var (longTermExpiredFunds, shortTermExpiredFunds) = expiredFunds.GetExpiredFunds(
                fundsIn.ToCalendarPeriodDictionary(),
                fundsOut.ToCalendarPeriodDictionary(),
                existingExpiredFunds
                    .Where(fund => fund.TransactionType == 5)
                    .ToCalendarPeriodDictionary(),
                existingExpiredFunds
                    .Where(fund => fund.TransactionType == 6)
                    .ToCalendarPeriodDictionary(),
                configuration.FundsExpiryPeriod,
                now,
                configuration.FundsExpiryPolicyChangeDate,
                configuration.NewFundsExpiryPeriod);

            var currentCalendarPeriod = new CalendarPeriod(now.Year, now.Month);
            AddCurrentPeriodIfRequired(
                longTermExpiredFunds,
                existingExpiredFunds,
                currentCalendarPeriod,
                transactionType: 5);

            var processShortTermFunds = configuration.FundsExpiryPolicyChangeDate.HasValue
                                        && now > configuration.FundsExpiryPolicyChangeDate.Value;

            if (processShortTermFunds)
            {
                AddCurrentPeriodIfRequired(
                    shortTermExpiredFunds,
                    existingExpiredFunds,
                    currentCalendarPeriod,
                    transactionType: 6);
            }
            else
            {
                shortTermExpiredFunds.Clear();
            }

            if (longTermExpiredFunds.Count > 0)
            {
                await expiredFundsRepository.Create(
                    request.AccountId,
                    longTermExpiredFunds.ToExpiredFundsList(),
                    now);
            }

            if (shortTermExpiredFunds.Count > 0)
            {
                await expiredFundsRepository.Create(
                    request.AccountId,
                    shortTermExpiredFunds.ToExpiredFundsList(),
                    now,
                    transactionType: 6);
            }

            var fundsWereExpired = longTermExpiredFunds.Any(fund => fund.Value != 0m)
                                   || shortTermExpiredFunds.Any(fund => fund.Value != 0m);

            var response = new ExpireFundsResponse
            {
                AccountId = request.AccountId,
                CorrelationId = request.CorrelationId,
                FundsExpired = fundsWereExpired,
                LongTermExpiredFundsCount = longTermExpiredFunds.Count,
                ShortTermExpiredFundsCount = shortTermExpiredFunds.Count
            };

            logger.LogInformation(
                "[CorrelationId: {CorrelationId}] Expired {LongTermCount} long-term and {ShortTermCount} short-term month(s) of funds for AccountId {AccountId}. FundsExpired {FundsExpired}.",
                request.CorrelationId,
                response.LongTermExpiredFundsCount,
                response.ShortTermExpiredFundsCount,
                request.AccountId,
                response.FundsExpired);

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "[CorrelationId: {CorrelationId}] Failed to expire funds for AccountId {AccountId}.",
                request.CorrelationId,
                request.AccountId);

            throw;
        }
    }

    private static void AddCurrentPeriodIfRequired(
        IDictionary<CalendarPeriod, decimal> calculatedExpiredFunds,
        IEnumerable<ExpiredFund> existingExpiredFunds,
        CalendarPeriod currentCalendarPeriod,
        byte transactionType)
    {
        var currentPeriodExists = existingExpiredFunds.Any(fund =>
            fund.TransactionType == transactionType
            && fund.CalendarPeriodYear == currentCalendarPeriod.Year
            && fund.CalendarPeriodMonth == currentCalendarPeriod.Month);

        if (!currentPeriodExists && !calculatedExpiredFunds.ContainsKey(currentCalendarPeriod))
        {
            calculatedExpiredFunds.Add(currentCalendarPeriod, 0m);
        }
    }
}
