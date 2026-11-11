using System.Text;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public class CsvEstimatesFormatter : IEstimatesFormatter
{
    public string MimeType => "text/csv";

    public string FileExtension => "csv";

    public DownloadFormatType DownloadFormatType => DownloadFormatType.CSV;

    public byte[] GetFileData(IEnumerable<EstimatesDownloadLine> estimates)
    {
        var builder = new StringBuilder();

        builder.AppendLine(string.Join(",", EstimatesDownloadColumns.Headers));

        foreach (var estimate in estimates)
        {
            builder.AppendLine(string.Join(",", EstimatesDownloadColumns.GetValues(estimate, EstimatesDownloadColumns.NumberFormat)));
        }

        return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
    }
}
