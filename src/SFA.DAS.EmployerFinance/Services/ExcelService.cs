using System.IO;
using ClosedXML.Excel;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class ExcelService : IExcelService
{
    public Dictionary<string, string[][]> ReadExcelFile(byte[] fileData)
    {
        var workbookData = new Dictionary<string, string[][]>();

        using var stream = new MemoryStream(fileData);
        using var workbook = new XLWorkbook(stream);
        
        foreach (var worksheet in workbook.Worksheets)
        {
            var worksheetData = new List<string[]>();

            foreach (var row in worksheet.Rows())
            {
                var rowData = row.Cells().Select(x => x.Value.ToString()).ToArray();
                worksheetData.Add(rowData);
            }
                        
            workbookData.Add(worksheet.Name, worksheetData.ToArray());
        }

        return workbookData;
    }

    public byte[] CreateExcelFile(Dictionary<string, string[][]> worksheets)
    {
        if (!worksheets.Any())
        {
            throw new ArgumentException("Cannot create file with empty dictionary", nameof(worksheets));
        }

        using var stream = new MemoryStream();
        using (var workbook = new XLWorkbook())
        {
            foreach (var (worksheetName, worksheetData) in worksheets)
            {
                var worksheet = workbook.Worksheets.Add(worksheetName);
                
                for (var rowIndex = 0; rowIndex < worksheetData.Length; rowIndex++)
                {
                    for (var columnIndex = 0; columnIndex < worksheetData[rowIndex].Length; columnIndex++)
                    {
                        worksheet.Cell(rowIndex + 1, columnIndex + 1).Value = worksheetData[rowIndex][columnIndex];
                    }
                }
            } 
            
            workbook.SaveAs(stream);
        }

        stream.Position = 0;
        
        return stream.ToArray();
    }

    public byte[] CreateExcelFile(string worksheetName, IReadOnlyList<ExcelColumn> columns, IEnumerable<object[]> rows)
    {
        using var stream = new MemoryStream();
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add(worksheetName);

            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                worksheet.Cell(1, columnIndex + 1).Value = columns[columnIndex].Header;
            }

            var rowNumber = 2;
            foreach (var row in rows)
            {
                for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
                {
                    var cell = worksheet.Cell(rowNumber, columnIndex + 1);
                    cell.Value = XLCellValue.FromObject(row[columnIndex]);

                    if (columns[columnIndex].NumberFormat is { } numberFormat)
                    {
                        cell.Style.NumberFormat.Format = numberFormat;
                    }
                }

                rowNumber++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(stream);
        }

        stream.Position = 0;

        return stream.ToArray();
    }
}