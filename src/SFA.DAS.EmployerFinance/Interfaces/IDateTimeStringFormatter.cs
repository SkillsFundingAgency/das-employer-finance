using System;

namespace SFA.DAS.EmployerFinance.Interfaces
{
    public interface IDateTimeStringFormatter
    {
        string FinancialYearStringFor(DateTime dateTime);
    }
}