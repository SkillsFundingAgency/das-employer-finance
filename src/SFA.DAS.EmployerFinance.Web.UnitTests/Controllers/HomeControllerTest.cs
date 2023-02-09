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
        private HomeController _homeController;
        private const string ExpectedUrl = "https://localhost";

        [SetUp]
        public void Arrange()
        {
            var urlHelper = new Mock<IUrlActionHelper>();
            urlHelper.Setup(x => x.LegacyEasAction("")).Returns(ExpectedUrl);
            
            _homeController =
                new HomeController(Mock.Of<EmployerFinanceConfiguration>(), urlHelper.Object);
        }

        [Test]
        public void IndexRedirectsToPortalSite()
        {
            // Act
            var result = _homeController.Index() as RedirectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ExpectedUrl, result.Url);
        }
    }
}
