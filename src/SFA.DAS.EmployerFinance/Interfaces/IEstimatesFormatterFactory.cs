using SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;

namespace SFA.DAS.EmployerFinance.Interfaces;

public interface IEstimatesFormatterFactory
{
    IEstimatesFormatter GetEstimatesFormatterByType(DownloadFormatType format);
}
