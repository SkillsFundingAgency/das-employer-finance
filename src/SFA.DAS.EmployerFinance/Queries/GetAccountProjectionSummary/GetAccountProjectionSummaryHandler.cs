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

            var forecastDeclaration = declarations.Where(x => 
                (x.PayrollYear == currentPayrollYear && x.PayrollMonth == currentPayrollMonth)||
                (x.PayrollYear == previousPayrollYear && x.PayrollMonth == previousPayrollMonth))         
                .OrderByDescending(x=> x.PayrollYear )
                    .ThenByDescending(x => x.PayrollMonth )
                .FirstOrDefault();
          
            if (forecastDeclaration != null && forecastDeclaration.TotalAmount > 0)
            {
                return new GetAccountProjectionSummaryResult { AccountId = forecastDeclaration.AccountId, FundsIn = forecastDeclaration.TotalAmount * 12 };
            }          

            return new GetAccountProjectionSummaryResult { AccountId = query.AccountId, FundsIn = 0 };
        }

        private static (string, int) GetPayrollYearAndMonthForLastMonth(DateTime currentDate)
        {
            var currentMonth = currentDate.Month;
            var payrollMonth = currentMonth >= 4 ? currentMonth - 3 : currentMonth + 9;

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