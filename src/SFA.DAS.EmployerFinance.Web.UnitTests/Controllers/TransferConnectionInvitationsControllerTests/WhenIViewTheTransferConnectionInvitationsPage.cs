using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIViewTheTransferConnectionInvitationsPage
    {
        private TransferConnectionInvitationsController _controller;

        [SetUp]
        public void Arrange()
        {
            _controller = new TransferConnectionInvitationsController(null, null, null, Mock.Of<IEncodingService>());
        }

        [Test]
        public void ThenIShouldBeShownTheTransferConnectionInvitationPage()
        {
            var result = _controller.Index("ABC123") as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.EqualTo("ABC123"));
        }
    }
}