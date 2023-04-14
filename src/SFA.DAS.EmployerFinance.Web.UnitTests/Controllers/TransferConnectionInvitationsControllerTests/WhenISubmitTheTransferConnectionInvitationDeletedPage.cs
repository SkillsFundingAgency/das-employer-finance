using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheTransferConnectionInvitationDeletedPage
    {
        private const string AccountHashedId = "ABC123";
        private const string TransferHashedId = "XYZ123";

        private TransferConnectionInvitationsController _controller;
        private DeletedTransferConnectionInvitationViewModel _viewModel = new DeletedTransferConnectionInvitationViewModel();
        
        [SetUp]
        public void Arrange()
        {
            var urlHelper = new Mock<IUrlActionHelper>();
            urlHelper.Setup(x => x.EmployerAccountsAction("teams")).Returns($"/accounts/{AccountHashedId}/teams");
            _controller = new TransferConnectionInvitationsController(null, Mock.Of<IMediator>(), urlHelper.Object, Mock.Of<IEncodingService>());
        }

        [Test]
        public void ThenIShouldBeRedirectedToTransfersDashboardIfIChoseOption1()
        {
            _viewModel.Choice = "GoToTransfersPage";

            var result = _controller.Deleted(AccountHashedId,TransferHashedId,_viewModel) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("TransferConnections"));
        }

        [Test]
        public void ThenIShouldBeRedirectedToHomePageIfIChoseOption2()
        {
            _viewModel.Choice = "GoToHomepage";

            var result = _controller.Deleted(AccountHashedId,TransferHashedId,_viewModel) as RedirectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Url, Is.EqualTo($"/accounts/{AccountHashedId}/teams"));
        }
    }
}