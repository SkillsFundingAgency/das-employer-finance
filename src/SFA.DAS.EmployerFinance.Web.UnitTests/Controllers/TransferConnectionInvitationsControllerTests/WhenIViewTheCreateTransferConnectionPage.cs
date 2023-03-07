using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIRequestTheStartTransferConnectionInvitationPage
    {
        private TransferConnectionInvitationsController _controller;

        [SetUp]
        public void Arrange()
        {
            _controller = new TransferConnectionInvitationsController(null, null, null, null);
        }

        [Test]
        public void ThenIShouldBeShownTheStartTransferConnectionInvitationPage()
        {
            var result = _controller.Start("ABC123") as ViewResult;
            var model = result?.Model as StartTransferConnectionInvitationViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(model.HashedAccountId, Is.EqualTo("ABC123"));
        }
    }
}