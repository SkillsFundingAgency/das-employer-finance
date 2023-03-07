using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetLatestPendingReceivedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIViewTheOutstandingTransferConnectionInvitationPage
    {
        private TransferConnectionInvitationsController _controller;
        private IConfigurationProvider _configurationProvider;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private const string HashedAccountId = "ABC123";
        private const long AccountId = 345435;

        [SetUp]
        public void SetUp()
        {
            _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _configurationProvider.CreateMapper();
            _mediator = new Mock<IMediator>();
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object, null, encodingService.Object);
        }

        [Test]
        public async Task ThenShouldRedirectWhenHasOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = new TransferConnectionInvitationDto()
                });

            var actionResult = await _controller.Outstanding(HashedAccountId);

            Assert.AreEqual(typeof(RedirectToActionResult), actionResult.GetType());
        }

        [Test]
        public async Task ThenShouldRedirectToExpectedRouteWhenHasOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = new TransferConnectionInvitationDto()
                });

            var actionResult = await _controller.Outstanding(HashedAccountId) as RedirectToActionResult;

            CheckRoute(
                null,
                nameof(TransferConnectionInvitationsController.Receive),
                actionResult);
        }

        [Test]
        public async Task ThenShouldRedirectWhenDoesNotHaveOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = null
                });

            var actionResult = await _controller.Outstanding(HashedAccountId);

            Assert.AreEqual(typeof(RedirectToActionResult), actionResult.GetType());
        }

        [Test]
        public async Task ThenShouldRedirectToExpectedRouteWhenDoesNotHaveOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = null
                });

            var actionResult = await _controller.Outstanding(HashedAccountId) as RedirectToActionResult;

            CheckRoute(
                nameof(TransferConnectionsController),
                nameof(TransferConnectionInvitationsController.Index),
                actionResult);
        }

        private void CheckRoute(string expectedControllerName, string expectedActionName, RedirectToActionResult actualRoute)
        {
            if (!string.IsNullOrWhiteSpace(expectedControllerName) && expectedControllerName.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
            {
                expectedControllerName = expectedControllerName.Substring(0, expectedControllerName.Length - "Controller".Length);
            }

            Assert.AreEqual(expectedControllerName, actualRoute.ControllerName);
            Assert.AreEqual(expectedActionName, actualRoute.ActionName);
        }
    }
}