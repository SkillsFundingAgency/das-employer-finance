using AutoMapper;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionsControllerTests;

[TestFixture]
public class WhenIViewTheTransferRequestsComponent
{
    private TransferConnectionsController _controller;
    private GetTransferRequestsResponse _response;
    private Mock<ILogger<TransferConnectionsController>> _logger;
    private IConfigurationProvider _mapperConfig;
    private IMapper _mapper;
    private Mock<IMediator> _mediator;
    private const long AccountId = 123213;

    [SetUp]
    public void Arrange()
    {
        
        _response = new GetTransferRequestsResponse
        {
            TransferRequests = new List<TransferRequestDto>()
        };

        _logger = new Mock<ILogger<TransferConnectionsController>>();
        _mapperConfig = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
        _mapper = _mapperConfig.CreateMapper();
        _mediator = new Mock<IMediator>();
        _mediator.Setup(m => m.Send(It.Is<GetTransferRequestsQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None)).ReturnsAsync(_response);
            
        _controller = new TransferConnectionsController(_logger.Object, _mapper, _mediator.Object, Mock.Of<IEncodingService>(), Mock.Of<ICookieStorageService<FlashMessageViewModel>>(), Mock.Of<IHttpContextAccessor>());
    }

    [Test]
    public async Task ThenAGetTransferRequestsQueryShouldBeSent()
    {
        await _controller.TransferRequests(AccountId);

        _mediator.Verify(m => m.Send(It.Is<GetTransferRequestsQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenIShouldBeShownTheTransferRequestsComponent()
    {
        var model = await _controller.TransferRequests(AccountId);
            
        Assert.That(model, Is.Not.Null);
        Assert.That(model.TransferRequests, Is.EqualTo(_response.TransferRequests));
    }

    [Test]
    public async Task ThenExceptionShouldBeLoggedWhenExceptionIsThrown()
    {
        _mediator.Setup(m => m.Send(It.IsAny<GetTransferRequestsQuery>(), CancellationToken.None)).Throws<Exception>();

        var result = await _controller.TransferRequests(AccountId);

        Assert.That(result, Is.Null);

        _logger.Verify(x => x.Log(LogLevel.Warning,0,
            It.Is<It.IsAnyType>((message, type) => message.ToString().Contains("Failed to get transfer requests") && type.Name == "FormattedLogValues"),
            It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()
        ), Times.Once);
    }
}