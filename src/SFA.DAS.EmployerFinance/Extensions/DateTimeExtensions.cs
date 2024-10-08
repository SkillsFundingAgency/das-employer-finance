﻿namespace SFA.DAS.EmployerFinance.Extensions
{
    public static class DateTimeExtensions
    {
        // Start date of 20th April is specific to the domain and not the general UK tax year.
        private const int FinancialYearStartDay = 20;
        private const int FinancialYearStartMonth = 4;

        public static string ToFinancialYearString(this DateTime dateTime)
        {
            var financialYearStartDate = new DateTime(dateTime.Year, FinancialYearStartMonth, FinancialYearStartDay);
            if (dateTime < financialYearStartDate)
            {
                return $"{dateTime.Year - 1}/{dateTime:yy}";
            }
            else
            {
                return $"{dateTime.Year}/{dateTime.AddYears(1):yy}";
            }
        }

        public static string ToPayrollYearString(this DateTime dateTime)
        {
            var financialYearStartDate = new DateTime(dateTime.Year, FinancialYearStartMonth, FinancialYearStartDay);
            financialYearStartDate = DateTime.SpecifyKind(financialYearStartDate, DateTimeKind.Utc);

            if (dateTime < financialYearStartDate)
            {
                return $"{(dateTime.Year - 1) % 100:00}-{dateTime.Year % 100:00}";
            }
            else
            {
                return $"{dateTime.Year % 100:00}-{(dateTime.Year + 1) % 100:00}";
            }
        }
    }
}
