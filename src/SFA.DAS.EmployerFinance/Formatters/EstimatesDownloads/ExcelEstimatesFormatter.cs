using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public class ExcelEstimatesFormatter(IExcelService excelService) : IEstimatesFormatter
{
    private const string WorksheetName = "Estimates";
    private const string MonthFormat = "mmm-yy";
    private const string CurrencyFormat = "\"£\"#,##0.00;-\"£\"#,##0.00";

    private static readonly IReadOnlyList<ExcelColumn> Columns =
    [
        new(EstimatesDownloadColumns.Headers[0], MonthFormat),
        .. EstimatesDownloadColumns.Headers.Skip(1).Select(header => new ExcelColumn(header, CurrencyFormat))
    ];

    public string MimeType => "text/xlsx";

    public string FileExtension => "xlsx";

    public DownloadFormatType DownloadFormatType => DownloadFormatType.Excel;

    public byte[] GetFileData(IEnumerable<EstimatesDownloadLine> estimates)
    {
        return excelService.CreateExcelFile(WorksheetName, Columns, estimates.Select(EstimatesDownloadColumns.GetCellValues));
    }
}
