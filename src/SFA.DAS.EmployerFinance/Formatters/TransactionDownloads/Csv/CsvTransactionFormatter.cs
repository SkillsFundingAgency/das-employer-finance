using System.Text;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Formatters.TransactionDowloads.Csv;

public abstract class CsvTransactionFormatter
{
    public string MimeType => "text/csv";

    public string FileExtension => "csv";

    public DownloadFormatType DownloadFormatType => DownloadFormatType.CSV;

    public byte[] GetFileData(IEnumerable<TransactionDownloadLine> transactions, bool isNewVersion)
    {
        var builder = new StringBuilder();

        builder.AppendLine(CreateHeaders(isNewVersion));

        WriteRowsCsv(transactions, builder, isNewVersion);
			
        var csvContent = builder.ToString();

        return System.Text.Encoding.UTF8.GetBytes(csvContent);
    }

    protected abstract string CreateHeaders(bool isNewVersion);

    protected abstract void WriteRowsCsv(IEnumerable<TransactionDownloadLine> transactions, StringBuilder builder, bool isNewVersion);
}