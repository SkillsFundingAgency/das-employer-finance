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
            var previousPayrollYear = _currentDateTime.Now.AddYears(-1).ToPayrollYearString();

            var forecastDeclaration = declarations.Where(x =>
            (x.PayrollYear == currentPayrollYear && x.PayrollMonth >= currentPayrollMonth - 2)
            || (x.PayrollYear == previousPayrollYear && x.PayrollMonth > 10))
            .OrderByDescending(x => x.PayrollYear)
                .ThenByDescending(x => x.PayrollMonth)
                .Take(2)
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
    }
}