﻿using AutoMapper;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.GetApprovedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests;

[TestFixture]
public class WhenIViewTheApprovedTransferConnectionInvitationPage
{
    private const string HashedAccountId = "ABC123";
    private const long AccountId = 4567;
    private const string HashedTransferConnectionInvitationId = "XYZ567";
    private const long TransferConnectionInvitationId = 9876;
    private TransferConnectionInvitationsController _controller;
    private IConfigurationProvider _configurationProvider;
    private IMapper _mapper;
    private Mock<IMediator> _mediator;
    private GetApprovedTransferConnectionInvitationResponse _response;

    [SetUp]
    public void Arrange()
    {
        var fixture = new Fixture();
        _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
        _mapper = _configurationProvider.CreateMapper();
        _mediator = new Mock<IMediator>();
        _response = fixture.Create<GetApprovedTransferConnectionInvitationResponse>();

        _mediator.Setup(m =>
            m.Send(
                It.Is<GetApprovedTransferConnectionInvitationQuery>(c =>
                    c.AccountId.Equals(AccountId) &&
                    c.TransferConnectionInvitationId.Equals(TransferConnectionInvitationId)),
                CancellationToken.None)).ReturnsAsync(_response);
        var encodingService = new Mock<IEncodingService>();
        encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
        encodingService.Setup(x => x.Decode(HashedTransferConnectionInvitationId, EncodingType.TransferRequestId)).Returns(TransferConnectionInvitationId);
        _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object, Mock.Of<IUrlActionHelper>(), encodingService.Object);
    }

    [Test]
    public async Task ThenAGetApprovedTransferConnectionQueryShouldBeSent()
    {
        await _controller.Approved(HashedAccountId, HashedTransferConnectionInvitationId);

        _mediator.Verify(m => m.Send(It.Is<GetApprovedTransferConnectionInvitationQuery>(c =>
            c.AccountId.Equals(AccountId) &&
            c.TransferConnectionInvitationId.Equals(TransferConnectionInvitationId)), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenIShouldBeShownTheApprovedTransferConnectionInvitationPage()
    {
        var result = await _controller.Approved(HashedAccountId, HashedTransferConnectionInvitationId) as ViewResult;
        var model = result?.Model as ApprovedTransferConnectionInvitationViewModel;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.Null);
        Assert.That(model, Is.Not.Null);
    }
}