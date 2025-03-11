using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Helpers;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class AccountTransactionsOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountTransactionsOrchestrator> _logger;
    private readonly ILinkGeneratorWrapper _linkGenerator;

    public AccountTransactionsOrchestrator(IMediator mediator, ILogger<AccountTransactionsOrchestrator> logger, ILinkGeneratorWrapper linkGenerator)
    {
        _mediator = mediator;
        _logger = logger;
        _linkGenerator = linkGenerator;
    }

    public async Task<Transactions> GetAccountTransactions(string hashedAccountId, int year, int month)
    {
        _logger.LogInformation("Requesting account transactions for account {HashedAccountId}, year {Year} and month {Month}", hashedAccountId, year, month);

        year =  year == default ? DateTime.Now.Year : year;
        month = month == default ? DateTime.Now.Month : month;

        var data = await _mediator.Send(new GetEmployerAccountTransactionsQuery
        {
            FromDate = new DateTime(year, month, 1),
            ToDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
            HashedAccountId = hashedAccountId
        });

        var response = new Transactions
        {
            HasPreviousTransactions = data.AccountHasPreviousTransactions,
            Year = year,
            Month = month
        };

        response.AddRange(data.Data.TransactionLines.Select(x => ConvertToTransactionViewModel(hashedAccountId, x)));

        _logger.LogInformation("Received account transactions response for account {HashedAccountId}, year {Year} and month {Month}", hashedAccountId, year, month);
        return response;
    }

    public async Task<Transactions> QueryAccountTransactions(string hashedAccountId, DateTime fromDate, DateTime toDate)
    {
        _logger.LogInformation("Querying account transactions for account {HashedAccountId}, from {FromDate} to {ToDate}", hashedAccountId, fromDate.ToString(), toDate.ToString());

        var data = await _mediator.Send(new GetEmployerAccountTransactionsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            HashedAccountId = hashedAccountId
        });

        var response = new Transactions
        {
            HasPreviousTransactions = data.AccountHasPreviousTransactions,
            Year = fromDate.Year,
            Month = fromDate.Month
        };

        response.AddRange(data.Data.TransactionLines.Select(x => ConvertToTransactionViewModel(hashedAccountId, x)));

        _logger.LogInformation("Returning account transactions for account {HashedAccountId}, from {FromDate} to {ToDate}", hashedAccountId, fromDate.ToString(), toDate.ToString());
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

    private Transaction ConvertToTransactionViewModel(string hashedAccountId, Models.Transaction.TransactionLine transactionLine)
    {
        var viewModel = new Transaction
        {
            Amount = transactionLine.Amount,
            Balance = transactionLine.Balance,
            Description = transactionLine.Description,
            TransactionType = (TransactionItemType)transactionLine.TransactionType,
            DateCreated = transactionLine.DateCreated,
            SubTransactions = transactionLine.SubTransactions?.Select(x => ConvertToTransactionViewModel(hashedAccountId, x)).ToList(),
            TransactionDate = transactionLine.TransactionDate
        };

        if (transactionLine.TransactionType == Models.Transaction.TransactionItemType.Declaration)
        {
            viewModel.ResourceUri = _linkGenerator.GetPathByName("GetLevyForPeriod", new { hashedAccountId, payrollYear = transactionLine.PayrollYear, payrollMonth = transactionLine.PayrollMonth });
        }

        return viewModel;
    }
}