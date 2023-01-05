using System;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Formatters
{
    public class DateTimeStringFormatter : IDateTimeStringFormatter
    {
        private readonly ICurrentDateTime _currentDateTime;

        // Start date of 20th April is specific to the domain and not the general UK tax year.
        private const int FinancialYearStartDay = 20;
        private const int FinancialYearStartMonth = 4;

        public DateTimeStringFormatter(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public string FinancialYearStringFor(DateTime dateTime)
        {
            var financialYearStartDate = new DateTime(_currentDateTime.Now.Year, FinancialYearStartMonth, FinancialYearStartDay);

            if (dateTime < financialYearStartDate)
            {
                return $"{dateTime.Year - 1}/{dateTime:yy}";
            }

            return $"{dateTime.Year}/{dateTime.AddYears(1):yy}";
        }
    }
}
