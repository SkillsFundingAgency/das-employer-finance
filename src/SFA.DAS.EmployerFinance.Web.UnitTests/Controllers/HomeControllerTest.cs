using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Helpers;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTest
    {
        private EmployerFinanceConfiguration _configuration;
        private HomeController _homeController;

        [SetUp]
        public void Arrange()
        {
            _configuration = new EmployerFinanceConfiguration
            {
                EmployerPortalBaseUrl = "https://localhost"
            };

            _homeController =
                new HomeController(Mock.Of<EmployerFinanceConfiguration>(), Mock.Of<IUrlActionHelper>());
        }

        [Test]
        public void IndexRedirectsToPortalSite()
        {
            // Act
            var result = _homeController.Index() as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(_configuration.EmployerPortalBaseUrl, result.Url);
        }
    }
}
