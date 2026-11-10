using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public class ExcelEstimatesFormatter(IExcelService excelService) : IEstimatesFormatter
{
    private const string WorksheetName = "Estimates";

    public string MimeType => "text/xlsx";

    public string FileExtension => "xlsx";

    public DownloadFormatType DownloadFormatType => DownloadFormatType.Excel;

    public byte[] GetFileData(IEnumerable<EstimatesDownloadLine> estimates)
    {
        var excelRows = new List<string[]> { EstimatesDownloadColumns.Headers };

        excelRows.AddRange(estimates.Select(EstimatesDownloadColumns.GetValues));

        return excelService.CreateExcelFile(new Dictionary<string, string[][]>
        {
            { WorksheetName, excelRows.ToArray() }
        });
    }
}
