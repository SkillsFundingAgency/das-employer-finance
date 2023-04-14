using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Commands.ApproveTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Commands.RejectTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheReceiveTransferConnectionInvitationPage
    {
        private const string HashedAccountId = "ABC123";
        private const long AccountId = 4567;
        private const string HashedTransferConnectionInvitationId = "XYZ567";
        private const int TransferConnectionInvitationId = 9876;

        private TransferConnectionInvitationsController _controller;
        private ReceiveTransferConnectionInvitationViewModel _viewModel;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            encodingService.Setup(x => x.Decode(HashedTransferConnectionInvitationId, EncodingType.TransferRequestId)).Returns(TransferConnectionInvitationId);
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new []
                {
                    new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier,Guid.NewGuid().ToString())
                }
            ));
            _controller = new TransferConnectionInvitationsController(null, _mediator.Object,
                Mock.Of<IUrlActionHelper>(), encodingService.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = user}
            };

            _viewModel = new ReceiveTransferConnectionInvitationViewModel
            {
                TransferConnectionInvitationId = HashedTransferConnectionInvitationId
            };
        }

        [Test]
        public async Task ThenAnApproveTransferConnectionCommandShouldBeSentIfIChoseOption1()
        {
            _viewModel.Choice = "Approve";
            _viewModel.TransferConnectionInvitationId = HashedTransferConnectionInvitationId;

            await _controller.Receive(HashedAccountId,HashedTransferConnectionInvitationId, _viewModel);

            _mediator.Verify(m => m.Send(It.Is<ApproveTransferConnectionInvitationCommand>(c => c.TransferConnectionInvitationId == TransferConnectionInvitationId), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheApprovedTransferConnectionPageIfIChoseOption1()
        {
            _viewModel.Choice = "Approve";

            var result = await _controller.Receive(HashedAccountId,HashedTransferConnectionInvitationId, _viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Approved"));
            Assert.That(result.RouteValues.TryGetValue("transferConnectionInvitationId", out var transferConnectionId), Is.True);
            Assert.That(transferConnectionId, Is.EqualTo(HashedTransferConnectionInvitationId));
        }

        [Test]
        public async Task ThenAnApproveTransferConnectionCommandShouldNotBeSentIfIChoseOption2()
        {
            _viewModel.Choice = "Reject";

            await _controller.Receive(HashedAccountId,HashedTransferConnectionInvitationId, _viewModel);

            _mediator.Verify(m => m.Send(It.IsAny<ApproveTransferConnectionInvitationCommand>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ThenARejectTransferConnectionCommandShouldBeSentIfIChoseOption2()
        {
            _viewModel.Choice = "Reject";

            await _controller.Receive(HashedAccountId,HashedTransferConnectionInvitationId, _viewModel);

            _mediator.Verify(m => m.Send(It.Is<RejectTransferConnectionInvitationCommand>(c => c.TransferConnectionInvitationId == TransferConnectionInvitationId), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheRejectedTransferConnectionPageIfIChoseOption2()
        {
            _viewModel.Choice = "Reject";

            var result = await _controller.Receive(HashedAccountId,HashedTransferConnectionInvitationId, _viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Rejected"));
            Assert.That(result.RouteValues.TryGetValue("transferConnectionInvitationId", out var transferConnectionId), Is.True);
            Assert.That(transferConnectionId, Is.EqualTo(HashedTransferConnectionInvitationId));
        }

        [Test]
        public async Task ThenARejectTransferConnectionCommandShouldNotBeSentIfIChoseOption1()
        {
            _viewModel.Choice = "Approve";

            await _controller.Receive(HashedAccountId, HashedTransferConnectionInvitationId, _viewModel);

            _mediator.Verify(m => m.Send(It.IsAny<RejectTransferConnectionInvitationCommand>(), CancellationToken.None), Times.Never);
        }
    }
}