using SFA.DAS.EmployerFinance.Formatters;
using SFA.DAS.EmployerFinance.Interfaces;
using StructureMap;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class DateTimeStringFormatterRegistry : Registry
    {
        public DateTimeStringFormatterRegistry()
        {
            For<IDateTimeStringFormatter>().Use<DateTimeStringFormatter>();
        }
    }
}
