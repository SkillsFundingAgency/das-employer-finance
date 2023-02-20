using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Commands.DeleteTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheTransferConnectionInvitationDetailsPage
    {
        private const long TransferConnectionId = 123;

        private TransferConnectionInvitationsController _controller;
        private TransferConnectionInvitationViewModel _viewModel;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<long>>(), CancellationToken.None));

            _controller =
                new TransferConnectionInvitationsController(null, _mediator.Object, Mock.Of<IUrlActionHelper>());

            _viewModel = new TransferConnectionInvitationViewModel
            {
                TransferConnectionInvitationId = 123
            };
        }

        [Test]
        public async Task ThenADeleteTransferConnectionInvitationCommandShouldBeSentIfIChoseOption1()
        {
           _viewModel.Choice = "Confirm";

            await _controller.Details(_viewModel);

            _mediator.Verify(m => m.Send(It.Is<DeleteTransferConnectionInvitationCommand>(c => c.TransferConnectionInvitationId == _viewModel.TransferConnectionInvitationId), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToDeleteConfirmedPageIfIChoseOption1()
        {
            _viewModel.Choice = "Confirm";

            var result = await _controller.Details(_viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Deleted"));
            Assert.That(result.ControllerName, Is.Null);
        }

        [Test]
        public async Task ThenADeleteTransferConnectionInvitationCommandShouldNotBeSentIfIChoseOption2()
        {
            _viewModel.Choice = "GoToTransfersPage";

            await _controller.Details(_viewModel);

            _mediator.Verify(m => m.Send(It.IsAny<DeleteTransferConnectionInvitationCommand>(), CancellationToken.None), Times.Never);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheTransfersPageIfIChoseOption2()
        {
            _viewModel.Choice = "GoToTransfersPage";

            var result = await _controller.Details(_viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("TransferConnections"));
        }
    }
}