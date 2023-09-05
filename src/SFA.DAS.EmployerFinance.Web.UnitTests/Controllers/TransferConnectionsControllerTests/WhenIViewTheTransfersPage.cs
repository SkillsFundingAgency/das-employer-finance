﻿using AutoMapper;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.Encoding;
using IAuthenticationService = SFA.DAS.Authentication.IAuthenticationService;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionsControllerTests;

[TestFixture]
public class WhenIViewTheTransfersPage
{
    private TransferConnectionsController _controller;
    private Mock<IMediator> _mediatorMock;
    private Mock<IAuthenticationService> _authenticationServiceMock;
    private Mock<IMapper> _mapper;
    private const long AccountId = 123124124;

    [SetUp]
    public void Arrange()
    {
            
        _mediatorMock = new Mock<IMediator>();
        _mapper = new Mock<IMapper>();
        _authenticationServiceMock = new Mock<IAuthenticationService>();
        var encodingService = new Mock<IEncodingService>();
        encodingService.Setup(x => x.Decode("ABC123", EncodingType.AccountId)).Returns(AccountId);
        _controller = new TransferConnectionsController(null, _mapper.Object, _mediatorMock.Object, encodingService.Object, Mock.Of<ICookieStorageService<FlashMessageViewModel>>(), _authenticationServiceMock.Object);
    }

    //TODO MAC-192 - Test more
    [Test]
    public async Task ThenIShouldBeShownTheTransfersPage()
    {
        // Arrange
        var fixture = new Fixture();
        _mediatorMock
            .Setup(mock => mock.Send(It.Is<GetTransferAllowanceQuery>(c=>c.AccountId==AccountId), CancellationToken.None))
            .ReturnsAsync(new GetTransferAllowanceResponse{TransferAllowance =  new TransferAllowance{RemainingTransferAllowance = 0,StartingTransferAllowance = 0}, TransferAllowancePercentage = 0.25m});
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetTransferConnectionInvitationAuthorizationQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetTransferConnectionInvitationAuthorizationResponse{ });
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetTransferConnectionInvitationsQuery>(), CancellationToken.None))
            .ReturnsAsync(fixture.Create<GetTransferConnectionInvitationsResponse>() );
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetTransferRequestsQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetTransferRequestsResponse() );
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetEmployerAccountDetailByHashedIdQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetEmployerAccountDetailByHashedIdResponse
            {
                AccountDetail = new AccountDetailDto
                {
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy
                }
            } );
        _mapper.Setup(x => x.Map<TransferAllowanceViewModel>(It.IsAny<GetTransferAllowanceResponse>()))
            .Returns(new TransferAllowanceViewModel());
        _mapper.Setup(x => x.Map<TransferConnectionInvitationAuthorizationViewModel>(It.IsAny<GetTransferConnectionInvitationAuthorizationResponse>()))
            .Returns(new TransferConnectionInvitationAuthorizationViewModel());
        _mapper.Setup(x => x.Map<TransferConnectionInvitationsViewModel>(It.IsAny<GetTransferConnectionInvitationsResponse>()))
            .Returns(new TransferConnectionInvitationsViewModel());
        _mapper.Setup(x => x.Map<TransferRequestsViewModel>(It.IsAny<GetTransferRequestsResponse>()))
            .Returns(new TransferRequestsViewModel());

        //Act
        var result = await _controller.Index("ABC123") as ViewResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.Null);
        Assert.That(result.Model, Is.Not.Null);
    }


    [Test]
    public async Task ThenUserDetailsAreUpserted()
    {
        // Arrange
        var fixture = new Fixture();
        _mediatorMock
            .Setup(mock => mock.Send(It.Is<GetTransferAllowanceQuery>(c => c.AccountId == AccountId), CancellationToken.None))
            .ReturnsAsync(new GetTransferAllowanceResponse { TransferAllowance = new TransferAllowance { RemainingTransferAllowance = 0, StartingTransferAllowance = 0 }, TransferAllowancePercentage = 0.25m });
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetTransferConnectionInvitationAuthorizationQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetTransferConnectionInvitationAuthorizationResponse { });
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetTransferConnectionInvitationsQuery>(), CancellationToken.None))
            .ReturnsAsync(fixture.Create<GetTransferConnectionInvitationsResponse>());
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetTransferRequestsQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetTransferRequestsResponse());
        _mediatorMock
            .Setup(mock => mock.Send(It.IsAny<GetEmployerAccountDetailByHashedIdQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetEmployerAccountDetailByHashedIdResponse
            {
                AccountDetail = new AccountDetailDto
                {
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy
                }
            });
        _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertRegisteredUserCommand>(), CancellationToken.None));
        _mapper.Setup(x => x.Map<TransferAllowanceViewModel>(It.IsAny<GetTransferAllowanceResponse>()))
            .Returns(new TransferAllowanceViewModel());
        _mapper.Setup(x => x.Map<TransferConnectionInvitationAuthorizationViewModel>(It.IsAny<GetTransferConnectionInvitationAuthorizationResponse>()))
            .Returns(new TransferConnectionInvitationAuthorizationViewModel());
        _mapper.Setup(x => x.Map<TransferConnectionInvitationsViewModel>(It.IsAny<GetTransferConnectionInvitationsResponse>()))
            .Returns(new TransferConnectionInvitationsViewModel());
        _mapper.Setup(x => x.Map<TransferRequestsViewModel>(It.IsAny<GetTransferRequestsResponse>()))
            .Returns(new TransferRequestsViewModel());
       
        var userRef = fixture.Create<string>();
        var email = fixture.Create<string>();
        var firstName = fixture.Create<string>();
        var lastName = fixture.Create<string>();

        _authenticationServiceMock.Setup(x => x.GetClaimValue(ControllerConstants.UserRefClaimKeyName)).Returns(userRef);
        _authenticationServiceMock.Setup(x => x.GetClaimValue(ControllerConstants.EmailClaimKeyName)).Returns(email);
        _authenticationServiceMock.Setup(x => x.GetClaimValue(DasClaimTypes.GivenName)).Returns(firstName);
        _authenticationServiceMock.Setup(x => x.GetClaimValue(DasClaimTypes.FamilyName)).Returns(lastName);

        //Act
        await _controller.Index("ABC123");

        // Assert
        _mediatorMock.Verify(x => x.Send(It.Is<UpsertRegisteredUserCommand>(u =>
                    u.UserRef == userRef &&
                    u.EmailAddress == email &&
                    u.FirstName == firstName &&
                    u.LastName == lastName),
                CancellationToken.None),
            Times.Once);
    }
}