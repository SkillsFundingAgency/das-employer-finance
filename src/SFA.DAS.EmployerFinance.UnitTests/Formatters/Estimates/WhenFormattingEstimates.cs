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
        CoInvestmentCosts = 75m,
        DfeCoInvestmentCost = 675m
    };

    private static readonly string[] ExpectedHeaders =
    [
        "Month", "Levy balance start of month", "Learner costs",
        "Completion payments", "Transfer costs", "Co-investment", "DfE co-investment contribution",
        "Levy in", "Expired levy", "Levy balance end of month"
    ];

    private static readonly string[] CoInvestmentHeaders = ["Co-investment", "DfE co-investment contribution"];

    private static readonly string[] ExpectedValues = ["Nov-26", "£10,000.00", "-£400.00", "-£100.00", "-£250.25", "£75.00", "£675.00", "£1,500.50", "-£50.00", "£10,700.25"];

    [Test]
    public void ThenTheCsvHasTheHeadersAndOneRowPerMonthInOrder()
    {
        var formatter = new CsvEstimatesFormatter();

        var lines = System.Text.Encoding.UTF8.GetString(formatter.GetFileData([Estimate]))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().HaveCount(2);
        lines[0].Should().Be(string.Join(",", ExpectedHeaders));
        lines[1].Should().Be("Nov-26,10000.00,-400.00,-100.00,-250.25,75.00,675.00,1500.50,-50.00,10700.25");
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
        rows[0].Should().Equal(new DateTime(2026, 11, 1), 10000m, -400m, -100m, -250.25m, 75m, 675m, 1500.5m, -50m, 10700.25m);
        formatter.DownloadFormatType.Should().Be(DownloadFormatType.Excel);
        formatter.FileExtension.Should().Be("xlsx");
    }

    [Test]
    public void ThenTheCsvHasNoCoInvestmentColumnsWhenNoMonthIsInCoInvestment()
    {
        var formatter = new CsvEstimatesFormatter();
        var estimates = new[]
        {
            new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), LevyBalanceStartOfMonth = 100m, LevyIn = 10m, LevyBalanceEndOfMonth = 110m, DfeCoInvestmentCost = null },
            new EstimatesDownloadLine { Month = new DateOnly(2026, 12, 1), LevyBalanceStartOfMonth = 110m, LevyIn = 10m, LevyBalanceEndOfMonth = 120m, DfeCoInvestmentCost = null }
        };

        var lines = System.Text.Encoding.UTF8.GetString(formatter.GetFileData(estimates))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().Equal(
            string.Join(",", ExpectedHeaders.Except(CoInvestmentHeaders)),
            "Nov-26,100.00,0.00,0.00,0.00,10.00,0.00,110.00",
            "Dec-26,110.00,0.00,0.00,0.00,10.00,0.00,120.00");
    }

    [Test]
    public void ThenTheCsvKeepsTheCoInvestmentColumnsWhenAnyMonthIsInCoInvestment()
    {
        var formatter = new CsvEstimatesFormatter();
        var estimates = new[]
        {
            new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), CoInvestmentCosts = null },
            new EstimatesDownloadLine { Month = new DateOnly(2026, 12, 1), CoInvestmentCosts = 50m, DfeCoInvestmentCost = 450m }
        };

        var lines = System.Text.Encoding.UTF8.GetString(formatter.GetFileData(estimates))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().Equal(
            string.Join(",", ExpectedHeaders),
            "Nov-26,0.00,0.00,0.00,0.00,,,0.00,0.00,0.00",
            "Dec-26,0.00,0.00,0.00,0.00,50.00,450.00,0.00,0.00,0.00");
    }

    [Test]
    public void ThenTheExcelWorksheetHasNoCoInvestmentColumnsWhenNoMonthIsInCoInvestment()
    {
        var excelService = new Mock<IExcelService>();
        IReadOnlyList<ExcelColumn> columns = null;
        List<object[]> rows = null;
        excelService
            .Setup(x => x.CreateExcelFile("Estimates", It.IsAny<IReadOnlyList<ExcelColumn>>(), It.IsAny<IEnumerable<object[]>>()))
            .Callback<string, IReadOnlyList<ExcelColumn>, IEnumerable<object[]>>((_, c, r) => { columns = c; rows = r.ToList(); })
            .Returns([1]);
        var formatter = new ExcelEstimatesFormatter(excelService.Object);
        var estimate = new EstimatesDownloadLine
        {
            Month = new DateOnly(2026, 11, 1), LevyBalanceStartOfMonth = 100m, LearnerCosts = -20m, LevyIn = 10m, ExpiredLevy = -5m, LevyBalanceEndOfMonth = 85m
        };

        formatter.GetFileData([estimate]);

        columns.Select(x => x.Header).Should().Equal(ExpectedHeaders.Except(CoInvestmentHeaders));
        columns[0].NumberFormat.Should().Be("mmm-yy");
        rows.Should().ContainSingle();
        rows[0].Should().Equal(new DateTime(2026, 11, 1), 100m, -20m, 0m, 0m, 10m, -5m, 85m);
    }

    [Test]
    public void ThenCoInvestmentIsLeftEmptyWhenThereIsNone()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), CoInvestmentCosts = null, DfeCoInvestmentCost = null };

        EstimatesDownloadColumns.GetValues(estimate, includeCoInvestment: true)[5..7].Should().AllSatisfy(x => x.Should().BeEmpty());
        EstimatesDownloadColumns.GetCellValues(estimate, includeCoInvestment: true)[5..7].Should().AllSatisfy(x => x.Should().BeNull());
    }

    [Test]
    public void ThenCoInvestmentIsShownWhenTheEmployerHasNothingToPay()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), CoInvestmentCosts = 0m, DfeCoInvestmentCost = 0m };

        EstimatesDownloadColumns.GetValues(estimate, includeCoInvestment: true)[5..7].Should().Equal("0.00", "0.00");
        EstimatesDownloadColumns.GetCellValues(estimate, includeCoInvestment: true)[5..7].Should().Equal(0m, 0m);
    }

    [Test]
    public void ThenDfeCoInvestmentIsShownToTwoDecimalPlacesAfterCoInvestment()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), CoInvestmentCosts = 13.37m, DfeCoInvestmentCost = 120.333m };

        EstimatesDownloadColumns.GetValues(estimate, includeCoInvestment: true)[6].Should().Be("120.33");
        EstimatesDownloadColumns.GetCellValues(estimate, includeCoInvestment: true)[6].Should().Be(120.333m);
    }

    [Test]
    public void ThenMoneyIsShownToTwoDecimalPlaces()
    {
        var estimate = new EstimatesDownloadLine { Month = new DateOnly(2026, 11, 1), LevyIn = 10.005m, LearnerCosts = -20.3333m };

        var values = EstimatesDownloadColumns.GetValues(estimate, includeCoInvestment: true);

        values[7].Should().Be("10.01");
        values[2].Should().Be("-20.33");
    }
}
