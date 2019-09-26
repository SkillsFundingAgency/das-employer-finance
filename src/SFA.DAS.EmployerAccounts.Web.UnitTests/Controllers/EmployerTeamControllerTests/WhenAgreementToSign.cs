﻿using Moq;
using NUnit.Framework;
using SFA.DAS.Authentication;
using SFA.DAS.Authorization;
using SFA.DAS.EAS.Portal.Client;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Web.Controllers;
using SFA.DAS.EmployerAccounts.Web.FeatureToggles;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using System.Web.Mvc;
using Model = SFA.DAS.EAS.Portal.Client.Types;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests.WhenHomePageToggleIsEnabled
{
    public class WhenAgreementToSign
    {
        private EmployerTeamController _controller;

        private Mock<IAuthenticationService> mockAuthenticationService;
        private Mock<IAuthorizationService> mockAuthorizationService;
        private Mock<IMultiVariantTestingService> mockMultiVariantTestingService;
        private Mock<ICookieStorageService<FlashMessageViewModel>> mockCookieStorageService;
        private Mock<EmployerTeamOrchestrator> mockEmployerTeamOrchestrator;
        private Mock<IPortalClient> mockPortalClient;
        private Mock<IBooleanToggleValueProvider> mockFeatureToggleProvider;

        [SetUp]
        public void Arrange()
        {
            mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthorizationService = new Mock<IAuthorizationService>();
            mockMultiVariantTestingService = new Mock<IMultiVariantTestingService>();
            mockCookieStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
            mockEmployerTeamOrchestrator = new Mock<EmployerTeamOrchestrator>();
            mockPortalClient = new Mock<IPortalClient>();
            mockFeatureToggleProvider = new Mock<IBooleanToggleValueProvider>();

            FeatureToggles.Features.BooleanToggleValueProvider = mockFeatureToggleProvider.Object;
            mockFeatureToggleProvider.Setup(m => m.EvaluateBooleanToggleValue(It.IsAny<IFeatureToggle>())).Returns(false);

           _controller = new EmployerTeamController(
                mockAuthenticationService.Object,
                mockAuthorizationService.Object,
                mockMultiVariantTestingService.Object,
                mockCookieStorageService.Object,
                mockEmployerTeamOrchestrator.Object,
                mockPortalClient.Object);
        }

        [Test]
        public void ThenTheSignAgreementViewIsReturnedAtRow1Panel1()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.AgreementsToSign = true;
            model.AccountViewModel = new Model.Account();
            model.AccountViewModel.Providers.Add(new Model.Provider());

            //Act
            var result = _controller.Row1Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("SignAgreement", (result.Model as dynamic).ViewName);
        }

        [Test]
        public void ThenTheTasksViewIsReturnedAtRow1Panel2()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.AgreementsToSign = true;

            model.AccountViewModel = new Model.Account();
            model.AccountViewModel.Providers.Add(new Model.Provider());

            //Act
            var result = _controller.Row1Panel2(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Tasks", (result.Model as dynamic).ViewName);
        }

        [Test]
        public void ThenTheDashboardViewIsReturnedAtRow2Panel1()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.AgreementsToSign = true;

            model.AccountViewModel = new Model.Account();
            model.AccountViewModel.Providers.Add(new Model.Provider());

            //Act
            var result = _controller.Row2Panel1(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Dashboard", (result.Model as dynamic).ViewName);
        }

        [Test]
        public void ThenTheEmptyViewIsReturnedAtRow2Panel2()
        {
            // Arrange
            var model = new AccountDashboardViewModel();
            model.PayeSchemeCount = 1;
            model.AgreementsToSign = true;

            model.AccountViewModel = new Model.Account();
            model.AccountViewModel.Providers.Add(new Model.Provider());

            //Act
            var result = _controller.Row2Panel2(model) as PartialViewResult;

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Empty", (result.Model as dynamic).ViewName);
        }
    }
}
