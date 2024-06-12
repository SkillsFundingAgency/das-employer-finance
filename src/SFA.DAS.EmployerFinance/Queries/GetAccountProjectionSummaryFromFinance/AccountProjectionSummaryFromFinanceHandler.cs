using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummaryFromFinance
{
    internal class AccountProjectionSummaryFromFinanceHandler : IRequestHandler<AccountProjectionSummaryFromFinanceQuery,
    AccountProjectionSummaryFromFinanceResult>
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<AccountProjectionSummaryFromFinanceHandler> _logger;
        private readonly IDasLevyRepository _repository;

        public AccountProjectionSummaryFromFinanceHandler(
            IOuterApiClient outerApiClient,
            IDasLevyRepository repository,
            ILogger<AccountProjectionSummaryFromFinanceHandler> logger)
        {
            _outerApiClient = outerApiClient;
            _repository = repository;
            _logger = logger;
        }

        public async Task<AccountProjectionSummaryFromFinanceResult> Handle(AccountProjectionSummaryFromFinanceQuery query, CancellationToken cancellationToken)
        {
            var declarations = await _repository.GetAccountLevyDeclarations(query.AccountId);

            var (currentPayrollYear, currentPayrollMonth) = GetCurrentFinancialYearAndMonth();
            var (previousPayrollYear, previousPayrollMonth) = GetPreviousFinancialYearAndMonth(currentPayrollYear, currentPayrollMonth);

            var currentMonthRecord = declarations
            .FirstOrDefault(record => record.PayrollYear == currentPayrollYear && record.PayrollMonth == currentPayrollMonth);

            if (currentMonthRecord != null && currentMonthRecord.TotalAmount > 0)
            {
                return new AccountProjectionSummaryFromFinanceResult { AccountId = currentMonthRecord.AccountId, FundsIn = currentMonthRecord.TotalAmount };
            }

            var previousMonthRecord = declarations
                .FirstOrDefault(record => record.PayrollYear == previousPayrollYear && record.PayrollMonth == previousPayrollMonth);

            if (previousMonthRecord != null && previousMonthRecord.TotalAmount > 0)
            {
                return new AccountProjectionSummaryFromFinanceResult { AccountId = previousMonthRecord.AccountId, FundsIn = previousMonthRecord.TotalAmount };
            }

            return new AccountProjectionSummaryFromFinanceResult { AccountId = query.AccountId, FundsIn = 0 };
        }

        private static (string, int) GetCurrentFinancialYearAndMonth()
        {
            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            int financialMonth = (currentMonth - 3 + 12) % 12 + 1;

            return (now.ToFinancialYearString(), financialMonth);
        }

        private static (string, int) GetPreviousFinancialYearAndMonth(string currentPayrollYear, int currentPayrollMonth)
        {
            int previousPayrollMonth = (currentPayrollMonth == 1 ? 12 : currentPayrollMonth - 1);

            string previousPayrollYear;
            if (previousPayrollMonth == 12)
            {
                int startYear = int.Parse(currentPayrollYear.Substring(0, 2));
                int endYear = int.Parse(currentPayrollYear.Substring(3, 2));
                previousPayrollYear = $"{startYear - 1}-{endYear - 1}";
            }
            else
            {
                previousPayrollYear = currentPayrollYear;
            }

            return (previousPayrollYear, previousPayrollMonth);
        }
    }
}