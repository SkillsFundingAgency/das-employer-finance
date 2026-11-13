using SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.UnitTests.Factories.EstimatesFormatterFactory;

public class WhenIUseTheEstimatesFormatterFactory
{
    private IEstimatesFormatterFactory _estimatesFormatterFactory;
    private Mock<IEstimatesFormatter> _csvFormatter;
    private Mock<IEstimatesFormatter> _excelFormatter;

    [SetUp]
    public void Arrange()
    {
        _csvFormatter = new Mock<IEstimatesFormatter>();
        _excelFormatter = new Mock<IEstimatesFormatter>();

        _csvFormatter.Setup(x => x.DownloadFormatType).Returns(DownloadFormatType.CSV);
        _excelFormatter.Setup(x => x.DownloadFormatType).Returns(DownloadFormatType.Excel);

        _estimatesFormatterFactory = new EmployerFinance.Formatters.EstimatesFormatterFactory(new List<IEstimatesFormatter>
        {
            _csvFormatter.Object,
            _excelFormatter.Object
        });
    }

    [Test]
    public void ThenIShouldGetCsvFormatter()
    {
        var formatter = _estimatesFormatterFactory.GetEstimatesFormatterByType(DownloadFormatType.CSV);

        formatter.Should().Be(_csvFormatter.Object);
    }

    [Test]
    public void ThenIShouldGetExcelFormatter()
    {
        var formatter = _estimatesFormatterFactory.GetEstimatesFormatterByType(DownloadFormatType.Excel);

        formatter.Should().Be(_excelFormatter.Object);
    }
}
