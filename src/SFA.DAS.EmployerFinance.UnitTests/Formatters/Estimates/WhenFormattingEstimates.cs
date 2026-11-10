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
        LevyIn = 1500m,
        LearnerCosts = -400m,
        CompletionPayments = -100m,
        TransferCosts = -250m,
        ExpiredLevy = -50m,
        LevyBalanceEndOfMonth = 10700m
    };

    private static readonly string[] ExpectedHeaders =
    [
        "Month", "Levy balance start of month", "Levy in", "Learner costs", "Completion payments",
        "Transfer costs", "Expired levy", "Levy balance end of month"
    ];

    private static readonly string[] ExpectedValues = ["November 2026", "10000", "1500", "-400", "-100", "-250", "-50", "10700"];

    [Test]
    public void ThenTheCsvHasTheHeadersAndOneRowPerMonthInOrder()
    {
        var formatter = new CsvEstimatesFormatter();

        var lines = System.Text.Encoding.UTF8.GetString(formatter.GetFileData([Estimate]))
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        lines.Should().HaveCount(2);
        lines[0].Should().Be(string.Join(",", ExpectedHeaders));
        lines[1].Should().Be(string.Join(",", ExpectedValues));
        formatter.DownloadFormatType.Should().Be(DownloadFormatType.CSV);
        formatter.FileExtension.Should().Be("csv");
    }

    [Test]
    public void ThenTheExcelWorksheetHasTheHeadersAndOneRowPerMonthInOrder()
    {
        var excelService = new Mock<IExcelService>();
        Dictionary<string, string[][]> worksheets = null;
        excelService
            .Setup(x => x.CreateExcelFile(It.IsAny<Dictionary<string, string[][]>>()))
            .Callback<Dictionary<string, string[][]>>(x => worksheets = x)
            .Returns([1, 2, 3]);
        var formatter = new ExcelEstimatesFormatter(excelService.Object);

        var result = formatter.GetFileData([Estimate]);

        result.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        worksheets.Should().ContainKey("Estimates");
        worksheets["Estimates"].Should().HaveCount(2);
        worksheets["Estimates"][0].Should().Equal(ExpectedHeaders);
        worksheets["Estimates"][1].Should().Equal(ExpectedValues);
        formatter.DownloadFormatType.Should().Be(DownloadFormatType.Excel);
        formatter.FileExtension.Should().Be("xlsx");
    }
}
