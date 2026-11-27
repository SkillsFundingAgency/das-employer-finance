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
        var lines = estimates.ToList();
        var includeCoInvestment = EstimatesDownloadColumns.HasCoInvestment(lines);

        var builder = new StringBuilder();

        builder.AppendLine(ToCsvField(EstimatesDownloadColumns.Disclaimer));
        builder.AppendLine(string.Join(",", EstimatesDownloadColumns.GetHeaders(includeCoInvestment)));

        foreach (var estimate in lines)
        {
            builder.AppendLine(string.Join(",", EstimatesDownloadColumns.GetValues(estimate, includeCoInvestment)));
        }

        return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string ToCsvField(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
