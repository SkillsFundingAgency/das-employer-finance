using SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Estimates;
using SFA.DAS.EmployerFinance.Queries.GetEstimatesDownload;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetEstimatesDownloadTests;

[TestFixture]
public class WhenIGetEstimatesDownload
{
    private const long AccountId = 111111;

    private Mock<IOuterApiService> _outerApiService;
    private Mock<IEstimatesFormatter> _csvFormatter;
    private Mock<IEstimatesFormatter> _excelFormatter;
    private List<EstimatesDownloadLine> _lines;
    private GetEstimatesDownloadQueryHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _outerApiService = new Mock<IOuterApiService>();
        _outerApiService
            .Setup(x => x.GetLevyProjectionsForDownload(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Summary = new GetLevySummaryByAccountIdResponse { CurrentLevyFunds = 1000.40m },
                Projections =
                [
                    CreateProjection(11, 2026, levyIn: 500.10m, learnerCost: 300.20m, completionCost: 100.30m, transferCost: 50.40m, transferCompletionCost: 10m, expiredLevy: 20.50m, closingBalance: 1019.20m),
                    CreateProjection(12, 2026, levyIn: 0m, learnerCost: 1500m, completionCost: 0m, transferCost: 0m, transferCompletionCost: 0m, expiredLevy: 0m, closingBalance: -480.80m, coInvestment: 120.25m),
                    CreateProjection(1, 2027, levyIn: 600m, learnerCost: 0m, completionCost: 0m, transferCost: 0m, transferCompletionCost: 0m, expiredLevy: 0m, closingBalance: 119.20m, coInvestment: 0m),
                ]
            });

        _csvFormatter = CreateFormatter(DownloadFormatType.CSV, "csv");
        _excelFormatter = CreateFormatter(DownloadFormatType.Excel, "xlsx");

        _handler = new GetEstimatesDownloadQueryHandler(_outerApiService.Object, [_csvFormatter.Object, _excelFormatter.Object]);
    }

    [TestCase(DownloadFormatType.CSV, "csv")]
    [TestCase(DownloadFormatType.Excel, "xlsx")]
    public async Task ThenTheFormatterForTheChosenFormatIsUsed(DownloadFormatType format, string expectedExtension)
    {
        var result = await _handler.Handle(new GetEstimatesDownloadQuery(AccountId, format), CancellationToken.None);

        result.FileExtension.Should().Be(expectedExtension);
        result.FileData.Should().BeEquivalentTo(new byte[] { 1 });
    }

    [Test]
    public async Task ThenThereIsOneLinePerProjectedMonthInOrder()
    {
        await _handler.Handle(new GetEstimatesDownloadQuery(AccountId, DownloadFormatType.CSV), CancellationToken.None);

        _lines.Select(x => x.Month).Should().Equal(new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1));
    }

    [Test]
    public async Task ThenTheFirstMonthStartsWithTheCurrentLevyFunds()
    {
        await _handler.Handle(new GetEstimatesDownloadQuery(AccountId, DownloadFormatType.CSV), CancellationToken.None);

        _lines[0].Should().BeEquivalentTo(new EstimatesDownloadLine
        {
            Month = new DateOnly(2026, 11, 1),
            LevyBalanceStartOfMonth = 1000.40m,
            LevyIn = 500.10m,
            LearnerCosts = -300.20m,
            CompletionPayments = -100.30m,
            TransferCosts = -60.40m,
            ExpiredLevy = -20.50m,
            LevyBalanceEndOfMonth = 1019.20m
        });
    }

    [Test]
    public async Task ThenEachMonthStartsWithThePreviousMonthsClosingBalance()
    {
        await _handler.Handle(new GetEstimatesDownloadQuery(AccountId, DownloadFormatType.CSV), CancellationToken.None);

        _lines[1].LevyBalanceStartOfMonth.Should().Be(_lines[0].LevyBalanceEndOfMonth);
    }

    [Test]
    public async Task ThenCoInvestmentIsPassedThroughIncludingWhenThereIsNone()
    {
        await _handler.Handle(new GetEstimatesDownloadQuery(AccountId, DownloadFormatType.CSV), CancellationToken.None);

        _lines.Select(x => x.CoInvestmentCosts).Should().Equal(null, 120.25m, 0m);
    }

    [Test]
    public async Task ThenBalancesBelowZeroAreShownAsZero()
    {
        await _handler.Handle(new GetEstimatesDownloadQuery(AccountId, DownloadFormatType.CSV), CancellationToken.None);

        _lines[1].LevyBalanceEndOfMonth.Should().Be(0m);
        _lines[2].LevyBalanceStartOfMonth.Should().Be(0m);
        _lines[2].LevyBalanceEndOfMonth.Should().Be(119.20m);
    }

    private Mock<IEstimatesFormatter> CreateFormatter(DownloadFormatType format, string extension)
    {
        var formatter = new Mock<IEstimatesFormatter>();
        formatter.Setup(x => x.DownloadFormatType).Returns(format);
        formatter.Setup(x => x.FileExtension).Returns(extension);
        formatter
            .Setup(x => x.GetFileData(It.IsAny<IEnumerable<EstimatesDownloadLine>>()))
            .Callback<IEnumerable<EstimatesDownloadLine>>(x => _lines = x.ToList())
            .Returns([1]);
        return formatter;
    }

    private static MonthlyFundingBreakdown CreateProjection(int month, int year, decimal levyIn, decimal learnerCost, decimal completionCost,
        decimal transferCost, decimal transferCompletionCost, decimal expiredLevy, decimal closingBalance, decimal? coInvestment = null) => new()
    {
        Month = month,
        Year = year,
        LevyInSummary = new LevyInSummary { Total = levyIn, LevyIn = levyIn },
        LevyOutSummary = new LevyOutSummary
        {
            CommittedLearnerCost = learnerCost,
            CommittedLearnerFinalPaymentCost = completionCost,
            CommittedTransferOut = transferCost,
            CommittedTransferOutFinalPaymentCost = transferCompletionCost,
            ExpiredLevy = expiredLevy
        },
        ClosingBalance = closingBalance,
        CoInvestmentCosts = coInvestment
    };
}
