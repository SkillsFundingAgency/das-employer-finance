using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public class ExcelEstimatesFormatter(IExcelService excelService) : IEstimatesFormatter
{
    private const string WorksheetName = "Estimates";
    private const string MonthFormat = "mmm-yy";
    private const string CurrencyFormat = "\"£\"#,##0.00;-\"£\"#,##0.00";

    public string MimeType => "text/xlsx";

    public string FileExtension => "xlsx";

    public DownloadFormatType DownloadFormatType => DownloadFormatType.Excel;

    public byte[] GetFileData(IEnumerable<EstimatesDownloadLine> estimates)
    {
        var lines = estimates.ToList();
        var includeCoInvestment = EstimatesDownloadColumns.HasCoInvestment(lines);

        return excelService.CreateExcelFile(
            WorksheetName,
            GetColumns(includeCoInvestment),
            lines.Select(x => EstimatesDownloadColumns.GetCellValues(x, includeCoInvestment)),
            EstimatesDownloadColumns.Disclaimer);
    }

    private static List<ExcelColumn> GetColumns(bool includeCoInvestment)
    {
        var headers = EstimatesDownloadColumns.GetHeaders(includeCoInvestment);

        return
        [
            new(headers[0], MonthFormat),
            .. headers.Skip(1).Select(header => new ExcelColumn(header, CurrencyFormat))
        ];
    }
}
