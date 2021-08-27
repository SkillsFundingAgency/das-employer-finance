﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Web.Controllers;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerAccounts.Web.Models;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests
{
    public class WhenIViewTermsAndCondition : ControllerTestBase
    {
        private Mock<IAuthenticationService> _owinWrapper;
        private Mock<HomeOrchestrator> _homeOrchestrator;
        private EmployerAccountsConfiguration _configuration;
        private Mock<IMultiVariantTestingService> _userViewTestingService;
        private HomeController _homeController;
        private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;

        [SetUp]
        public void Arrage()
        {
            base.Arrange();

            _owinWrapper = new Mock<IAuthenticationService>();
            _homeOrchestrator = new Mock<HomeOrchestrator>();
            _userViewTestingService = new Mock<IMultiVariantTestingService>();
            _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
            _configuration = new EmployerAccountsConfiguration();

            _homeController = new HomeController(
                _owinWrapper.Object,
                _homeOrchestrator.Object,
                _configuration,
                _userViewTestingService.Object,
                _flashMessage.Object,
                Mock.Of<ICookieStorageService<ReturnUrlModel>>(),
                Mock.Of<ILog>())
            {
                ControllerContext = _controllerContext.Object
            };
        }

        [Test]
        public void ThenTheViewIsReturned()
        {
            //Act
            var actual = _homeController.TermsAndConditions("returnUrl", "hashedId");

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<ViewResult>(actual);
        }
    }
}
