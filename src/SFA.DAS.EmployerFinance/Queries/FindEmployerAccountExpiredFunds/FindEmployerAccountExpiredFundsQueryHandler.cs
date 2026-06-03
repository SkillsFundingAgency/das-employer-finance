using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.FindEmployerAccountExpiredFunds;

public class FindEmployerAccountExpiredFundsQueryHandler : IRequestHandler<FindEmployerAccountExpiredFundsQuery, FindEmployerAccountExpiredFundsResponse>
{
    private readonly IValidator<FindEmployerAccountExpiredFundsQuery> _validator;
    private readonly IDasLevyService _dasLevyService;
    private readonly IEncodingService _encodingService;

    public FindEmployerAccountExpiredFundsQueryHandler(
        IValidator<FindEmployerAccountExpiredFundsQuery> validator, 
        IDasLevyService dasLevyService,
        IEncodingService encodingService)
    {
        _validator = validator;
        _dasLevyService = dasLevyService;
        _encodingService = encodingService;
    }
    
    public async Task<FindEmployerAccountExpiredFundsResponse> Handle(FindEmployerAccountExpiredFundsQuery request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }
        
        var accountId = _encodingService.Decode(request.HashedAccountId, EncodingType.AccountId);
        var transactions = await _dasLevyService.GetAccountTransactionsByDateRange
            (accountId, request.FromDate, request.ToDate);
        
        var expiredFunds = transactions.Where(x=>x.TransactionType is TransactionItemType.ExpiredFund or TransactionItemType.ShortExpiredFund).ToList();

        return new FindEmployerAccountExpiredFundsResponse
        {
            Total = expiredFunds.Sum(x => x.Amount),
            TwelveMonthExpiryAmount = expiredFunds.Where(c => c.TransactionType == TransactionItemType.ShortExpiredFund)
                .Sum(x => x.Amount),
            TwentyFourthMonthExpiryAmount = expiredFunds
                .Where(c => c.TransactionType == TransactionItemType.ExpiredFund).Sum(x => x.Amount),
            TransactionDate = expiredFunds.First().TransactionDate
        };
    }
}