using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Events.ProcessPayment;

namespace SFA.DAS.EmployerFinance.UnitTests.Events.ProcessPaymentTests;

public class WhenIProcessPaymentData
{
    private ProcessPaymentEventHandler _eventHandler;
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private Mock<ILogger<ProcessPaymentEventHandler>> _logger;

    [SetUp]
    public void Arrange()
    {
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _logger = new Mock<ILogger<ProcessPaymentEventHandler>>();

        _eventHandler = new ProcessPaymentEventHandler(_dasLevyRepository.Object, _logger.Object);
    }

    [Test]
    public async Task ThenTheProcessDeclarationsRepositoryCallIsMade()
    {
        //Arrange
        const int accountId = 10;

        //Act
        await _eventHandler.Handle(new ProcessPaymentEvent{AccountId = accountId }, CancellationToken.None);

        //Assert
        _dasLevyRepository.Verify(x => x.ProcessPaymentData(accountId), Times.Once);
    }

    [Test]
    public async Task ThenTheLoggerIsCalledWithInfoLevel()
    {
        //Act
        await _eventHandler.Handle(new ProcessPaymentEvent(), CancellationToken.None);

        //Assert
        _logger.Verify(x => x.LogInformation("Process Payments Called"));
    }
}