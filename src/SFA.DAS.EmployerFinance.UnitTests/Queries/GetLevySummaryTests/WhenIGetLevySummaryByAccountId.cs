using SFA.DAS.EmployerFinance.Queries.GetLevySummary;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevySummaryTests;

[TestFixture]
public class WhenIGetLevySummary
{
    private Mock<IDasLevyService> _dasLevyService;
    private Mock<IEncodingService> _encodingService;
    private GetLevySummaryQueryHandler _handler;

    private const string ExpectedHashedAccountId = "ABC123";
    private const long ExpectedAccountId = 99887;
    private const decimal ExpectedAccountBalance = 5000.75m;

    [SetUp]
    public void Arrange()
    {
        _encodingService = new Mock<IEncodingService>();
        _encodingService
            .Setup(x => x.Decode(ExpectedHashedAccountId, EncodingType.AccountId))
            .Returns(ExpectedAccountId);

        _dasLevyService = new Mock<IDasLevyService>();
        _dasLevyService
            .Setup(x => x.GetAccountBalance(ExpectedAccountId))
            .ReturnsAsync(ExpectedAccountBalance);

        _handler = new GetLevySummaryQueryHandler(_dasLevyService.Object, _encodingService.Object);
    }

    [Test]
    public async Task ThenTheLevyServiceIsCalledWithTheDecodedAccountId()
    {
        //Act
        await _handler.Handle(new GetLevySummaryQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        _dasLevyService.Verify(x => x.GetAccountBalance(ExpectedAccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheEncodingServiceIsCalledWithTheHashedAccountId()
    {
        //Act
        await _handler.Handle(new GetLevySummaryQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        _encodingService.Verify(x => x.Decode(ExpectedHashedAccountId, EncodingType.AccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheResponseIsNotNull()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheResponseContainsTheLevySummary()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheCurrentLevyFundsIsSetToTheAccountBalance()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.CurrentLevyFunds.Should().Be(ExpectedAccountBalance);
    }

    [Test]
    public async Task ThenWhenTheAccountBalanceIsZeroItIsReflectedInTheSummary()
    {
        //Arrange
        _dasLevyService
            .Setup(x => x.GetAccountBalance(ExpectedAccountId))
            .ReturnsAsync(0m);

        //Act
        var result = await _handler.Handle(new GetLevySummaryQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.CurrentLevyFunds.Should().Be(0m);
    }
}