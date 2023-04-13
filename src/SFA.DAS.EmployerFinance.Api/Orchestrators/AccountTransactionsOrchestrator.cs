using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class AccountTransactionsOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountTransactionsOrchestrator> _logger;
    
    public AccountTransactionsOrchestrator(IMediator mediator, ILogger<AccountTransactionsOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
      
    public async Task<Transactions> GetAccountTransactions(string hashedAccountId, int year, int month, IUrlHelper urlHelper)
    {
        _logger.LogInformation("Requesting account transactions for account {HashedAccountId}, year {Year} and month {Month}", hashedAccountId, year, month);

        var data = await _mediator.Send(new GetEmployerAccountTransactionsQuery {
            Year = year,
            Month = month,
            HashedAccountId = hashedAccountId
        });

        var response = new Transactions
        {
            HasPreviousTransactions = data.AccountHasPreviousTransactions,
            Year = year,
            Month = month
        };
        response.AddRange(data.Data.TransactionLines.Select(x => ConvertToTransactionViewModel(hashedAccountId, x, urlHelper)));
            
        _logger.LogInformation("Received account transactions response for account {HashedAccountId}, year {Year} and month {Month}", hashedAccountId, year, month);
        return response;
    }

    public async Task<List<TransactionSummary>> GetAccountTransactionSummary(string hashedAccountId)
    {
        _logger.LogInformation("Requesting account transaction summary for account {HashedAccountId}", hashedAccountId);

        var response = await _mediator.Send(new GetAccountTransactionSummaryRequest { HashedAccountId = hashedAccountId });
        if (response.Data == null)
        {
            return null;
        }
        _logger.LogInformation("Received account transaction summary response for account {HashedAccountId}", hashedAccountId);
        return response.Data;
    }

    private static Transaction ConvertToTransactionViewModel(string hashedAccountId, Models.Transaction.TransactionLine transactionLine, IUrlHelper urlHelper)
    {
        var viewModel = new Transaction
        {
            Amount = transactionLine.Amount,
            Balance = transactionLine.Balance,
            Description = transactionLine.Description,
            TransactionType = (TransactionItemType)transactionLine.TransactionType,
            DateCreated = transactionLine.DateCreated,
            SubTransactions = transactionLine.SubTransactions?.Select(x => ConvertToTransactionViewModel(hashedAccountId, x, urlHelper)).ToList(),
            TransactionDate = transactionLine.TransactionDate
        };

        if (transactionLine.TransactionType == Models.Transaction.TransactionItemType.Declaration)
        {
            viewModel.ResourceUri = urlHelper.RouteUrl("GetLevyForPeriod", new { hashedAccountId, payrollYear = transactionLine.PayrollYear, payrollMonth = transactionLine.PayrollMonth });
        }

        return viewModel;
    }
}