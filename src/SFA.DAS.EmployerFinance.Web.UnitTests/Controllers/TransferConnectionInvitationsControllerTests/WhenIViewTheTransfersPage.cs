using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    public class WhenIViewTheTransfersPage
    {
        private TransferConnectionInvitationsController _controller;
        private readonly Mock<IMediator> _mediator = new Mock<IMediator>();
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>();

        [SetUp]
        public void Arrange()
        {
            _controller = new TransferConnectionInvitationsController(_mapper.Object, _mediator.Object, null, Mock.Of<IEncodingService>());
        }

        [Test]
        public void ThenIShouldBeShownTheTransferConnectionsPage()
        {
            var result = _controller.Index("ABC123") as ViewResult;
            var model = result?.Model;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(model, Is.EqualTo("ABC123"));
        }
    }
}