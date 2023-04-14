using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Queries.GetRejectedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIViewTheRejectedTransferConnectionInvitationPage
    {
        private const string HashedAccountId = "ABC123";
        private const long AccountId = 4567;
        private const string HashedTransferConnectionInvitationId = "XYZ567";
        private const long TransferConnectionInvitationId = 9876;
        private TransferConnectionInvitationsController _controller;
        private IConfigurationProvider _configurationProvider;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private GetRejectedTransferConnectionInvitationResponse _response;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _configurationProvider.CreateMapper();
            _mediator = new Mock<IMediator>();
            _response = fixture.Create<GetRejectedTransferConnectionInvitationResponse>();
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
            encodingService.Setup(x => x.Decode(HashedTransferConnectionInvitationId, EncodingType.TransferRequestId)).Returns(TransferConnectionInvitationId);
            _mediator.Setup(m =>
                m.Send(
                    It.Is<GetRejectedTransferConnectionInvitationQuery>(c =>
                        c.AccountId.Equals(AccountId) &&
                        c.TransferConnectionInvitationId.Equals(TransferConnectionInvitationId)),
                    CancellationToken.None)).ReturnsAsync(_response);

            _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object, null, encodingService.Object);
        }

        [Test]
        public async Task ThenAGetRejectedTransferConnectionQueryShouldBeSent()
        {
            await _controller.Rejected(HashedAccountId,HashedTransferConnectionInvitationId);

            _mediator.Verify(m => m.Send(It.Is<GetRejectedTransferConnectionInvitationQuery>(c =>
                c.AccountId.Equals(AccountId) &&
                c.TransferConnectionInvitationId.Equals(TransferConnectionInvitationId)), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeShownTheApprovedTransferConnectionInvitationPage()
        {
            var result = await _controller.Rejected(HashedAccountId,HashedTransferConnectionInvitationId) as ViewResult;
            var model = result?.Model as RejectedTransferConnectionInvitationViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(model, Is.Not.Null);
        }
    }
}