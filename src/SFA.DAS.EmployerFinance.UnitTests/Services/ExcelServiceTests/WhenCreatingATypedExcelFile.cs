using System.IO;
using ClosedXML.Excel;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EmployerFinance.UnitTests.Services.ExcelServiceTests;

[TestFixture]
public class WhenCreatingATypedExcelFile
{
    private const string MoneyFormat = "\"£\"#,##0.00;-\"£\"#,##0.00";

    [Test]
    public void ThenNumbersAndDatesAreStoredWithTheirColumnFormat()
    {
        var fileData = new ExcelService().CreateExcelFile(
            "Estimates",
            [new ExcelColumn("Month", "mmm-yy"), new ExcelColumn("Levy in", MoneyFormat), new ExcelColumn("Co-investment", MoneyFormat)],
            [[new DateTime(2026, 11, 1), 1500.5m, null], [new DateTime(2026, 12, 1), -400m, 0m]]);

        using var workbook = new XLWorkbook(new MemoryStream(fileData));
        var worksheet = workbook.Worksheet("Estimates");

        worksheet.Cell(1, 1).GetString().Should().Be("Month");
        worksheet.Cell(1, 2).GetString().Should().Be("Levy in");

        worksheet.Cell(2, 1).DataType.Should().Be(XLDataType.DateTime);
        worksheet.Cell(2, 1).GetDateTime().Should().Be(new DateTime(2026, 11, 1));
        worksheet.Cell(2, 1).Style.NumberFormat.Format.Should().Be("mmm-yy");

        worksheet.Cell(2, 2).DataType.Should().Be(XLDataType.Number);
        worksheet.Cell(2, 2).GetDouble().Should().Be(1500.5);
        worksheet.Cell(2, 2).Style.NumberFormat.Format.Should().Be(MoneyFormat);
        worksheet.Cell(3, 2).GetDouble().Should().Be(-400);

        worksheet.Cell(2, 3).IsEmpty().Should().BeTrue();
        worksheet.Cell(3, 3).DataType.Should().Be(XLDataType.Number);
        worksheet.Cell(3, 3).GetDouble().Should().Be(0);
    }
}
