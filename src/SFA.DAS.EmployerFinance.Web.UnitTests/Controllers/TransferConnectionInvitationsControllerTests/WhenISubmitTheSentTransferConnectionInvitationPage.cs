using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheSentTransferConnectionInvitationPage
    {
        private const string AccountHashedId = "ABC123";

        private TransferConnectionInvitationsController _controller;
        private readonly SentTransferConnectionInvitationViewModel _viewModel = new SentTransferConnectionInvitationViewModel();
        
        [SetUp]
        public void Arrange()
        {
            var urlHelper = new Mock<IUrlActionHelper>();
            urlHelper.Setup(x => x.EmployerAccountsAction("team")).Returns($"/accounts/{AccountHashedId}/teams");
            
            _controller = new TransferConnectionInvitationsController(null, Mock.Of<IMediator>(), urlHelper.Object);
        }

        [Test]
        public void ThenIShouldBeRedirectedToTheTransfersPageIfIChoseOption1()
        {
            _viewModel.Choice = "GoToTransfersPage";

            var result = _controller.Sent(_viewModel) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues.TryGetValue("action", out var actionName), Is.True);
            Assert.That(actionName, Is.EqualTo("Index"));
            Assert.That(result.RouteValues.TryGetValue("controller", out var controllerName), Is.True);
            Assert.That(controllerName, Is.EqualTo("TransferConnections"));
        }

        [Test]
        public void ThenIShouldBeRedirectedToTheHomepageIfIChoseOption2()
        {
            _viewModel.Choice = "GoToHomepage";

            var result = _controller.Sent(_viewModel) as RedirectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Url, Is.EqualTo($"/accounts/{AccountHashedId}/teams"));
        }
    }
}