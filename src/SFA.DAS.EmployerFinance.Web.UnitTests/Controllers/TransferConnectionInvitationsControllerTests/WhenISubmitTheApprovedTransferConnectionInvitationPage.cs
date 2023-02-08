﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheApprovedTransferConnectionInvitationPage
    {
        private const string AccountHashedId = "ABC123";

        private TransferConnectionInvitationsController _controller;
        private readonly ApprovedTransferConnectionInvitationViewModel _viewModel = new ApprovedTransferConnectionInvitationViewModel();
        private readonly Mock<IMediator> _mediator = new Mock<IMediator>();

        [SetUp]
        public void Arrange()
        {
            var urlHelper = new Mock<IUrlActionHelper>();
            urlHelper.Setup(x => x.EmployerAccountsAction("teams")).Returns($"/accounts/{AccountHashedId}/teams");
            urlHelper.Setup(x => x.EmployerCommitmentsV2Action("")).Returns($"/{AccountHashedId}");
            
            _controller = new TransferConnectionInvitationsController(null, _mediator.Object, urlHelper.Object);
        }

        [Test]
        public void ThenIShouldBeRedirectedToTheApprenticesPageIfIChoseOption1()
        {
            _viewModel.Choice = "GoToApprenticesPage";

            var result = _controller.Approved(_viewModel) as RedirectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Url, Is.EqualTo($"/{AccountHashedId}"));
        }

        [Test]
        public void ThenIShouldBeRedirectedToTheHomepageIfIChoseOption2()
        {
            _viewModel.Choice = "GoToHomepage";

            var result = _controller.Approved(_viewModel) as RedirectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Url, Is.EqualTo($"/accounts/{AccountHashedId}/teams"));
        }
    }
}