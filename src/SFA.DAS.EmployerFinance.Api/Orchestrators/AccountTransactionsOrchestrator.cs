using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators
{
    public class AccountTransactionsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;
        private readonly IUrlHelper _urlHelper;

        public AccountTransactionsOrchestrator(IMediator mediator, ILog logger, IUrlHelper urlHelper)
        {
            _mediator = mediator;
            _logger = logger;
            _urlHelper = urlHelper;
        }
      
        public async Task<Transactions> GetAccountTransactions(string hashedAccountId, int year, int month, IUrlHelper urlHelper)
        {
            _logger.Info($"Requesting account transactions for account {hashedAccountId}, year {year} and month {month}");

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
            
            _logger.Info($"Received account transactions response for account {hashedAccountId}, year {year} and month {month}");
            return response;
        }

        public async Task<List<TransactionSummary>> GetAccountTransactionSummary(string hashedAccountId)
        {
            _logger.Info($"Requesting account transaction summary for account {hashedAccountId}");

            var response = await _mediator.Send(new GetAccountTransactionSummaryRequest { HashedAccountId = hashedAccountId });
            if (response.Data == null)
            {
                return null;
            }
            _logger.Info($"Received account transaction summary response for account {hashedAccountId}");
            return response.Data;
        }

        private Transaction ConvertToTransactionViewModel(string hashedAccountId, Models.Transaction.TransactionLine transactionLine, IUrlHelper urlHelper)
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
}