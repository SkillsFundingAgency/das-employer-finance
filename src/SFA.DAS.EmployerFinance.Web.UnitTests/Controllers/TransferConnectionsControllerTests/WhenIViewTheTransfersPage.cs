using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransfersControllerTests
{
    [TestFixture]
    public class WhenIViewTheTransfersPage
    {
        private TransferConnectionsController _controller;
        private Mock<IMediator> _mediatorMock;
        private Mock<IMapper> _mapper;
        private const long AccountId = 123124124;

        [SetUp]
        public void Arrange()
        {
            _mediatorMock = new Mock<IMediator>();
            _mapper = new Mock<IMapper>();
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode("ABC123", EncodingType.AccountId)).Returns(AccountId);
            _controller = new TransferConnectionsController(null, _mapper.Object, _mediatorMock.Object, encodingService.Object);
        }

        //TODO MAC-192 - Test more
        [Test]
        public async Task ThenIShouldBeShownTheTransfersPage()
        {
            // Arrange
            _mediatorMock
                .Setup(mock => mock.Send(It.Is<GetTransferAllowanceQuery>(c=>c.AccountId==AccountId), CancellationToken.None))
                .ReturnsAsync(new GetTransferAllowanceResponse{TransferAllowance =  new TransferAllowance{RemainingTransferAllowance = 0,StartingTransferAllowance = 0}, TransferAllowancePercentage = 0.25m});
            _mediatorMock
                .Setup(mock => mock.Send(It.IsAny<GetTransferConnectionInvitationAuthorizationQuery>(), CancellationToken.None))
                .ReturnsAsync(new GetTransferConnectionInvitationAuthorizationResponse{ });
            _mediatorMock
                .Setup(mock => mock.Send(It.IsAny<GetTransferConnectionInvitationsQuery>(), CancellationToken.None))
                .ReturnsAsync(new GetTransferConnectionInvitationsResponse() );
            _mediatorMock
                .Setup(mock => mock.Send(It.IsAny<GetTransferRequestsQuery>(), CancellationToken.None))
                .ReturnsAsync(new GetTransferRequestsResponse() );
            _mapper.Setup(x => x.Map<TransferAllowanceViewModel>(It.IsAny<GetTransferAllowanceResponse>()))
                .Returns(new TransferAllowanceViewModel());

            //Act
            var result = await _controller.Index("ABC123") as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(result.Model, Is.Not.Null);
        }
    }
}