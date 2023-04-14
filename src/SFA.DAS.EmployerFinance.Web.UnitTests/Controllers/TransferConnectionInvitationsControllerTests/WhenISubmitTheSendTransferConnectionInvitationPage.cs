using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Commands.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheSendTransferConnectionInvitationPage
    {
        private const int TransferConnectionId = 123;

        private TransferConnectionInvitationsController _controller;
        private SendTransferConnectionInvitationViewModel _viewModel;
        private Mock<IMediator> _mediator;
        private const string HashedAccountId = "ABC123";
        private const long AccountId = 4567;
        private const string HashedTransferConnectionInvitationId = "XYZ567";
        private const long TransferConnectionInvitationId = 9876;
        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(It.IsAny<SendTransferConnectionInvitationCommand>(), CancellationToken.None)).ReturnsAsync(TransferConnectionId);
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            encodingService.Setup(x => x.Decode(HashedTransferConnectionInvitationId, EncodingType.TransferRequestId)).Returns(TransferConnectionInvitationId);
            encodingService.Setup(x => x.Encode(TransferConnectionId, EncodingType.TransferRequestId)).Returns(HashedTransferConnectionInvitationId);
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new []
                {
                    new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier,Guid.NewGuid().ToString())
                }
            ));
            _controller = new TransferConnectionInvitationsController(null, _mediator.Object, Mock.Of<IUrlActionHelper>(), encodingService.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
            
            _viewModel = new SendTransferConnectionInvitationViewModel
            {
                ReceiverAccountPublicHashedId = "ABC123"
            };
        }

        [Test]
        public async Task ThenASendTransferConnectionCommandShouldBeSentIfIChoseOption1()
        {
            _viewModel.Choice = "Confirm";

            await _controller.Send(HashedAccountId,_viewModel);

            _mediator.Verify(m => m.Send(It.Is<SendTransferConnectionInvitationCommand>(c => 
                c.ReceiverAccountPublicHashedId == _viewModel.ReceiverAccountPublicHashedId
                && c.AccountId.Equals(AccountId)
                ), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheSentTransferConnectionPageIfIChoseOption1()
        {
            _viewModel.Choice = "Confirm";

            var result = await _controller.Send(HashedAccountId,_viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Sent"));
            Assert.That(result.ControllerName, Is.Null);
            Assert.That(result.RouteValues.TryGetValue("transferConnectionInvitationId", out var transferConnectionId), Is.True);
            Assert.That(transferConnectionId, Is.EqualTo(HashedTransferConnectionInvitationId));
        }

        [Test]
        public async Task ThenASendTransferConnectionCommandShouldNotBeSentIfIChoseOption2()
        {
            _viewModel.Choice = "ReEnterAccountId";

            await _controller.Send(HashedAccountId,_viewModel);

            _mediator.Verify(m => m.Send(It.IsAny<SendTransferConnectionInvitationCommand>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToStartPageIfIChoseOption2()
        {
            _viewModel.Choice = "ReEnterAccountId";

            var result = await _controller.Send(HashedAccountId,_viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Start"));
            Assert.That(result.ControllerName, Is.Null);
        }
    }
}