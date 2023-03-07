using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Queries.GetReceivedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIViewTheReceiveTransferConnectionInvitationPage
    {
        private const string HashedAccountId = "ABC123";
        private const long AccountId = 4567;
        private const string HashedTransferConnectionInvitationId = "XYZ567";
        private const long TransferConnectionInvitationId = 9876;
        private TransferConnectionInvitationsController _controller;
        private IConfigurationProvider _configurationProvider;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private readonly GetReceivedTransferConnectionInvitationResponse _response = new GetReceivedTransferConnectionInvitationResponse();

        [SetUp]
        public void Arrange()
        {
            _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _configurationProvider.CreateMapper();
            _mediator = new Mock<IMediator>();

            _mediator.Setup(m => m.Send(It.Is<GetReceivedTransferConnectionInvitationQuery>(c =>
                c.AccountId.Equals(AccountId) &&
                c.TransferConnectionInvitationId.Value.Equals(TransferConnectionInvitationId)), CancellationToken.None)).ReturnsAsync(_response);
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            encodingService.Setup(x => x.Decode(HashedTransferConnectionInvitationId, EncodingType.TransferRequestId)).Returns(TransferConnectionInvitationId);

            _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object, null, encodingService.Object);
        }

        [Test]
        public async Task ThenAGetReceivedTransferConnectionInvitationQueryShouldBeSent()
        {
            await _controller.Receive(HashedAccountId, HashedTransferConnectionInvitationId);

            _mediator.Verify(
                m => m.Send(
                    It.Is<GetReceivedTransferConnectionInvitationQuery>(c =>
                        c.AccountId.Equals(AccountId) &&
                        c.TransferConnectionInvitationId.Value.Equals(TransferConnectionInvitationId)),
                    CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeShownTheReceiveTransferConnectionInvitationPage()
        {
            var result = await _controller.Receive(HashedAccountId, HashedTransferConnectionInvitationId) as ViewResult;
            var model = result?.Model as ReceiveTransferConnectionInvitationViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(model, Is.Not.Null);
        }
    }
}