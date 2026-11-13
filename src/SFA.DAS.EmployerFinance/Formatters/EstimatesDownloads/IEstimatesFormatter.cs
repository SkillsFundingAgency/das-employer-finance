using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public interface IEstimatesFormatter
{
    byte[] GetFileData(IEnumerable<EstimatesDownloadLine> estimates);

    string MimeType { get; }

    string FileExtension { get; }

    DownloadFormatType DownloadFormatType { get; }
}
