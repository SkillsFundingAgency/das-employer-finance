using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
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
using SFA.DAS.Notifications.Api.Types;
using System.Security.Claims;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionsControllerTests;

[TestFixture]
public class WhenIViewTheTransfersPage
{
    private TransferConnectionsController _controller;
    private Mock<IMediator> _mediatorMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private Mock<IMapper> _mapper;
    private const long AccountId = 123124124;

    [SetUp]
    public void Arrange()
    {
            
        _mediatorMock = new Mock<IMediator>();
        _mapper = new Mock<IMapper>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var encodingService = new Mock<IEncodingService>();
        encodingService.Setup(x => x.Decode("ABC123", EncodingType.AccountId)).Returns(AccountId);

        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        _controller = new TransferConnectionsController(Mock.Of<ILogger<TransferConnectionsController>>(), _mapper.Object, _mediatorMock.Object, encodingService.Object, Mock.Of<ICookieStorageService<FlashMessageViewModel>>(), _httpContextAccessorMock.Object);
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

        var claims = new List<Claim>
        {
            new(ControllerConstants.UserRefClaimKeyName, userRef),
            new(DasClaimTypes.Email, email),
            new(DasClaimTypes.GivenName, firstName),
            new(DasClaimTypes.FamilyName, lastName)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

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