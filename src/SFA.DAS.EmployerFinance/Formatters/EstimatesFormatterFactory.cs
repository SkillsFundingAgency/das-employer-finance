using SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Formatters;

public class EstimatesFormatterFactory(IEnumerable<IEstimatesFormatter> formatters) : IEstimatesFormatterFactory
{
    public IEstimatesFormatter GetEstimatesFormatterByType(DownloadFormatType format)
    {
        return formatters.FirstOrDefault(f => f.DownloadFormatType == format);
    }
}
