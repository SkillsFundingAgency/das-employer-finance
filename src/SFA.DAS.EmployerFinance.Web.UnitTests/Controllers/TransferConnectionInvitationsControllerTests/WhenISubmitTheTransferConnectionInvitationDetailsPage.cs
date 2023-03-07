using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Commands.DeleteTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheTransferConnectionInvitationDetailsPage
    {
        private TransferConnectionInvitationsController _controller;
        private TransferConnectionInvitationViewModel _viewModel;
        private Mock<IMediator> _mediator;
        private const string HashedAccountId = "ABC123";
        private const long AccountId = 4567;
        private const string HashedTransferConnectionInvitationId = "XYZ567";
        private const long TransferConnectionInvitationId = 9876;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<long>>(), CancellationToken.None));
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            encodingService.Setup(x => x.Decode(HashedTransferConnectionInvitationId, EncodingType.TransferRequestId)).Returns(TransferConnectionInvitationId);
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new []
                {
                    new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier,Guid.NewGuid().ToString())
                }
            ));
            _controller =
                new TransferConnectionInvitationsController(null, _mediator.Object, Mock.Of<IUrlActionHelper>(), encodingService.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
            _viewModel = new TransferConnectionInvitationViewModel
            {
                TransferConnectionInvitationId = 123
            };
        }

        [Test]
        public async Task ThenADeleteTransferConnectionInvitationCommandShouldBeSentIfIChoseOption1()
        {
           _viewModel.Choice = "Confirm";

            await _controller.Details(HashedAccountId, HashedTransferConnectionInvitationId, _viewModel);

            _mediator.Verify(
                m => m.Send(
                    It.Is<DeleteTransferConnectionInvitationCommand>(c =>
                        c.TransferConnectionInvitationId == TransferConnectionInvitationId &&
                        c.AccountId.Equals(AccountId)), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToDeleteConfirmedPageIfIChoseOption1()
        {
            _viewModel.Choice = "Confirm";

            var result = await _controller.Details(HashedAccountId, HashedTransferConnectionInvitationId, _viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Deleted"));
            Assert.That(result.ControllerName, Is.Null);
        }

        [Test]
        public async Task ThenADeleteTransferConnectionInvitationCommandShouldNotBeSentIfIChoseOption2()
        {
            _viewModel.Choice = "GoToTransfersPage";

            await _controller.Details(HashedAccountId, HashedTransferConnectionInvitationId,_viewModel);

            _mediator.Verify(m => m.Send(It.IsAny<DeleteTransferConnectionInvitationCommand>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheTransfersPageIfIChoseOption2()
        {
            _viewModel.Choice = "GoToTransfersPage";

            var result = await _controller.Details(HashedAccountId, HashedTransferConnectionInvitationId,_viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("TransferConnections"));
        }
    }
}