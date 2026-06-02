using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Formatters.TransactionDownloads.Excel;

public abstract class ExcelTransactionFormatter(IExcelService excelService)
{
    private const string WorksheetName = "Transactions";

    public string MimeType => "text/xlsx";
    public string FileExtension => "xlsx";
    public DownloadFormatType DownloadFormatType => DownloadFormatType.Excel;

    protected abstract string[] GetHeaderRow(bool isNewVersion);

    public byte[] GetFileData(IEnumerable<TransactionDownloadLine> transactions, bool isNewVersion)
    {
        var excelRows = new List<string[]> { GetHeaderRow(isNewVersion) };

        excelRows.AddRange(GetTransactionRows(transactions, isNewVersion));

        var transactionData = new Dictionary<string, string[][]>
        {
            {WorksheetName, excelRows.ToArray()}
        };

        var fileData = excelService.CreateExcelFile(transactionData);

        return fileData;
    }

    protected abstract IEnumerable<string[]> GetTransactionRows(IEnumerable<TransactionDownloadLine> transactions, bool isNewVersion);
}