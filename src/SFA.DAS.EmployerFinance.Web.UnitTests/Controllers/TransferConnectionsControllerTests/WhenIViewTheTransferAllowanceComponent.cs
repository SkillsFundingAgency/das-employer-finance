using AutoMapper;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionsControllerTests;

[TestFixture]
public class WhenIViewTheTransferAllowanceComponent
{
    private TransferConnectionsController _controller;
    private GetTransferAllowanceResponse _response;
    private IConfigurationProvider _mapperConfig;
    private IMapper _mapper;
    private Mock<IMediator> _mediator;
    private TransferAllowance _transferAllowance;
    private EmployerFinanceConfiguration _configuration;
    private const long AccountId = 23442;

    [SetUp]
    public void Arrange()
    {
        _configuration = new EmployerFinanceConfiguration { TransferAllowancePercentage = .25m };
        _transferAllowance = new TransferAllowance
        {
            RemainingTransferAllowance = 123.456m,
            StartingTransferAllowance = 234.56M,
        };
        _response = new GetTransferAllowanceResponse { TransferAllowance = _transferAllowance, TransferAllowancePercentage = _configuration.TransferAllowancePercentage };
        _mapperConfig = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
        _mapper = _mapperConfig.CreateMapper();
        _mediator = new Mock<IMediator>();
        _mediator.Setup(m => m.Send(It.Is<GetTransferAllowanceQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None)).ReturnsAsync(_response);
            
        _controller = new TransferConnectionsController(Mock.Of<ILogger<TransferConnectionsController>>(), _mapper, _mediator.Object, Mock.Of<IEncodingService>(), Mock.Of<IHttpContextAccessor>());
    }

    [Test]
    public async Task ThenAGetTransferAllowanceQueryShouldBeSent()
    {
        await _controller.TransferAllowance(AccountId);

        _mediator.Verify(m => m.Send(It.Is<GetTransferAllowanceQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenIShouldBeShownTheTransferAllowanceComponent()
    {
        var model = await _controller.TransferAllowance(AccountId);

        Assert.That(model, Is.Not.Null);
        Assert.That(model.RemainingTransferAllowance, Is.EqualTo(_response.TransferAllowance.RemainingTransferAllowance));
    }

    [Test]
    public async Task ThenIShouldBeShownTheCorrectTransferAllowancePercentage()
    {
        //Act
        var result = await _controller.TransferAllowance(AccountId);
            
        //Assert
        Assert.AreEqual(25m, result.TransferAllowancePercentage);
    }
}