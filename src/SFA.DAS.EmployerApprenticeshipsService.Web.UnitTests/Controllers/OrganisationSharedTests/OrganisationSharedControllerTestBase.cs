﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.Helpers;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.EAS.Web.ViewModels.Organisation;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.OrganisationSharedTests
{
    public abstract class OrganisationSharedControllerTestBase
    {
        private Mock<IOwinWrapper> _owinWrapper;
        private Mock<IFeatureToggle> _featureToggle;
        private Mock<IMultiVariantTestingService> _userViewTestingService;
        private Mock<ILog> _logger;
        private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
        protected OrganisationSharedController Controller;
        protected Mock<OrganisationOrchestrator> Orchestrator;
        protected Mock<IMapper> Mapper;

        [SetUp]
        public virtual void Arrange()
        {
            Orchestrator = new Mock<OrganisationOrchestrator>();
            _owinWrapper = new Mock<IOwinWrapper>();
            _featureToggle = new Mock<IFeatureToggle>();
            _userViewTestingService = new Mock<IMultiVariantTestingService>();
            Mapper = new Mock<IMapper>();
            _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();

            SetupOrchestrator();
            SetupMapper();

            _logger = new Mock<ILog>();

            Controller = new OrganisationSharedController(
                _owinWrapper.Object,
                Orchestrator.Object,
                _featureToggle.Object,
                _userViewTestingService.Object,
                Mapper.Object,
                _logger.Object,
                _flashMessage.Object);
        }

        public virtual void SetupOrchestrator()
        {
            
        }

        public virtual void SetupMapper()
        {
            
        }

        protected void SetupPopulatedRouteData()
        {
            var routeData = new RouteData();
            routeData.Values.Add(ControllerConstants.HashedAccountIdKeyName, "ABC123");
            Controller.ControllerContext = new ControllerContext(new Mock<HttpContextBase>().Object, routeData, Controller);
        }

        protected void SetupEmptyRouteData()
        {
            var routeData = new RouteData();
            Controller.ControllerContext = new ControllerContext(new Mock<HttpContextBase>().Object, routeData, Controller);
        }
    }
}