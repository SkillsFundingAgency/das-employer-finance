using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummary
{
    public class GetAccountProjectionSummaryHandler : IRequestHandler<GetAccountProjectionSummaryQuery,
    GetAccountProjectionSummaryResult>
    {
        private readonly ILogger<GetAccountProjectionSummaryHandler> _logger;
        private readonly IDasLevyRepository _repository;
        private readonly ICurrentDateTime _currentDateTime;

        public GetAccountProjectionSummaryHandler(
            IDasLevyRepository repository,
            ICurrentDateTime currentDateTime,
            ILogger<GetAccountProjectionSummaryHandler> logger)
        {
            _repository = repository;
            _currentDateTime = currentDateTime;
            _logger = logger;
        }

        public async Task<GetAccountProjectionSummaryResult> Handle(GetAccountProjectionSummaryQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GettingAccountProjectionSummary for accountId {Id}", query.AccountId);

            var declarations = await _repository.GetAccountLevyDeclarations(query.AccountId);

            var (currentPayrollYear, currentPayrollMonth) = GetPayrollYearAndMonthForLastMonth(_currentDateTime.Now);
            var (previousPayrollYear, previousPayrollMonth) = GetNextPreviousPayrollYearAndMonth(currentPayrollYear, currentPayrollMonth);

            var currentMonthRecord = declarations
            .Find(record => record.PayrollYear == currentPayrollYear && record.PayrollMonth == currentPayrollMonth);

            if (currentMonthRecord != null && currentMonthRecord.TotalAmount > 0)
            {
                return new GetAccountProjectionSummaryResult { AccountId = currentMonthRecord.AccountId, FundsIn = currentMonthRecord.TotalAmount * 12 };
            }

            var previousMonthRecord = declarations
                .Find(record => record.PayrollYear == previousPayrollYear && record.PayrollMonth == previousPayrollMonth);

            if (previousMonthRecord != null && previousMonthRecord.TotalAmount > 0)
            {
                return new GetAccountProjectionSummaryResult { AccountId = previousMonthRecord.AccountId, FundsIn = previousMonthRecord.TotalAmount * 12 };
            }

            return new GetAccountProjectionSummaryResult { AccountId = query.AccountId, FundsIn = 0 };
        }

        private static (string, int) GetPayrollYearAndMonthForLastMonth(DateTime currentDate)
        {
            var currentMonth = currentDate.Month;
            int payrollMonth = (currentMonth - 3 + 12) % 12 - 1;

            return (currentDate.ToPayrollYearString(), payrollMonth);
        }

        private static (string, int) GetNextPreviousPayrollYearAndMonth(string currentPayrollYear, int currentPayrollMonth)
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