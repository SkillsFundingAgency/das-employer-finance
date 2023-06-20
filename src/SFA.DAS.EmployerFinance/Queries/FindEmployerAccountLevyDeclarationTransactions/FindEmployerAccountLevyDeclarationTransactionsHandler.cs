using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Queries.FindEmployerAccountLevyDeclarationTransactions;

public class FindEmployerAccountLevyDeclarationTransactionsHandler : IRequestHandler<FindEmployerAccountLevyDeclarationTransactionsQuery, FindEmployerAccountLevyDeclarationTransactionsResponse>
{
    private readonly IValidator<FindEmployerAccountLevyDeclarationTransactionsQuery> _validator;
    private readonly IDasLevyService _dasLevyService;
    private readonly IEncodingService _encodingService;

    public FindEmployerAccountLevyDeclarationTransactionsHandler(
        IValidator<FindEmployerAccountLevyDeclarationTransactionsQuery> validator, 
        IDasLevyService dasLevyService,
        IEncodingService encodingService)
    {
        _validator = validator;
        _dasLevyService = dasLevyService;
        _encodingService = encodingService;
    }

    public async Task<FindEmployerAccountLevyDeclarationTransactionsResponse> Handle(FindEmployerAccountLevyDeclarationTransactionsQuery message,CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }
        
        var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var transactions = await _dasLevyService.GetAccountLevyTransactionsByDateRange<LevyDeclarationTransactionLine>
            (accountId, message.FromDate, message.ToDate);
            
        //var transactionDetailSummaries = data.Select(item => new LevyDeclarationTransactionLine
        //{
        //    Amount = item.Amount,
        //    EmpRef = item.EmpRef,
        //    TopUp = item.TopUp,
        //    TransactionDate = item.TransactionDate,
        //    EnglishFraction = item.EnglishFraction,
        //    LineAmount = item.LineAmount
        //}).ToList();

        return new FindEmployerAccountLevyDeclarationTransactionsResponse
        {
            Transactions = transactions.ToList(),
            Total = transactions.Sum(c => c.LineAmount)
        };
    }
}