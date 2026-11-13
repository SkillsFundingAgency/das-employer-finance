using System.Text;
using SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.UnitTests.Formatters.Estimates;

[TestFixture]
public class WhenFormattingEstimates
{
    private static readonly EstimatesDownloadLine Estimate = new()
    {
        Month = new DateOnly(2026, 11, 1),
        LevyBalanceStartOfMonth = 10000m,
        LevyIn = 1500.5m,
        LearnerCosts = -400m,
        CompletionPayments = -100m,
        TransferCosts = -250.25m,
        ExpiredLevy = -50m,
        LevyBalanceEndOfMonth = 10700.25m,
        CoInvestmentCosts = 75m
    };

    private static readonly string[] ExpectedHeaders =
    [
        "Month", "Starting levy", "Levy in", "Paid by levy", "Transfers sent",
        "Completion payments", "Expiry", "Remaining levy", "Co-investment"
    ];

    private static readonly string[] ExpectedValues = ["Nov-26", "£10,000.00", "£1,500.50", "-£400.00", "-£250.25", "-£100.00", "-£50.00", "£10,700.25", "£75.00"];

    [Test]
    public void ThenTheCsvHasTheHeadersAndOneRowPerMonthInOrder()
    {
        var formatter = new CsvEstimatesFormatter();

        var lines = System.Text.Encoding.UTF8.GetString(formatter.GetFileData([Estimate]))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().HaveCount(2);
        lines[0].Should().Be(string.Join(",", ExpectedHeaders));
        lines[1].Should().Be("Nov-26,10000.00,1500.50,-400.00,-250.25,-100.00,-50.00,10700.25,75.00");
        formatter.DownloadFormatType.Should().Be(DownloadFormatType.CSV);
        formatter.FileExtension.Should().Be("csv");
    }

    [Test]
    public void ThenTheExcelWorksheetHasTypedValuesInColumnOrder()
    {
        var excelService = new Mock<IExcelService>();
        IReadOnlyList<ExcelColumn> columns = null;
        List<object[]> rows = null;
        excelService
            .Setup(x => x.CreateExcelFile("Estimates", It.IsAny<IReadOnlyList<ExcelColumn>>(), It.IsAny<IEnumerable<object[]>>()))
            .Callback<string, IReadOnlyList<ExcelColumn>, IEnumerable<object[]>>((_, c, r) => { columns = c; rows = r.ToList(); })
            .Returns([1, 2, 3]);
        var formatter = new ExcelEstimatesFormatter(excelService.Object);

        var result = formatter.GetFileData([Estimate]);

        result.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        columns.Select(x => x.Header).Should().Equal(ExpectedHeaders);
        columns[0].NumberFormat.Should().Be("mmm-yy");
        columns.Skip(1).Should().AllSatisfy(x => x.NumberFormat.Should().Be("\"£\"#,##0.00;-\"£\"#,##0.00"));
        rows.Should().ContainSingle();
        rows[0].Should().Equal(new DateTime(2026, 11, 1), 10000m, 1500.5m, -400m, -250.25m, -100m, -50m, 10700.25m, 75m);
        formatter.DownloadFormatType.Should().Be(DownloadFormatType.Excel);
        formatter.FileExtension.Should().Be("xlsx");
    }

    [Test]
    public void ThenCoInvestmentIsLeftEmptyWhenThereIsNone()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), CoInvestmentCosts = null };

        EstimatesDownloadColumns.GetValues(estimate)[^1].Should().BeEmpty();
        EstimatesDownloadColumns.GetCellValues(estimate)[^1].Should().BeNull();
    }

    [Test]
    public void ThenCoInvestmentIsShownWhenTheEmployerHasNothingToPay()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), CoInvestmentCosts = 0m };

        EstimatesDownloadColumns.GetValues(estimate)[^1].Should().Be("0.00");
        EstimatesDownloadColumns.GetCellValues(estimate)[^1].Should().Be(0m);
    }

    [Test]
    public void ThenMoneyIsShownToTwoDecimalPlaces()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), LevyIn = 10.005m, LearnerCosts = -20.3333m };

        var values = EstimatesDownloadColumns.GetValues(estimate);

        values[2].Should().Be("10.01");
        values[3].Should().Be("-20.33");
    }
}
