namespace SFA.DAS.EmployerFinance.Interfaces;

public interface IExcelService
{
    byte[] CreateExcelFile(Dictionary<string, string[][]> worksheets);
    byte[] CreateExcelFile(string worksheetName, IReadOnlyList<ExcelColumn> columns, IEnumerable<object[]> rows, string title = null);
    Dictionary<string, string[][]> ReadExcelFile(byte[] fileData);
}

public sealed record ExcelColumn(string Header, string NumberFormat = null);
