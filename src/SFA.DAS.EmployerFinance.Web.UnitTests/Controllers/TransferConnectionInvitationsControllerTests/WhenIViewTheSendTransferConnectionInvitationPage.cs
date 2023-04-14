using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Queries.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIViewTheSendTransferConnectionInvitationPage
    {
        private TransferConnectionInvitationsController _controller;
        private IConfigurationProvider _configurationProvider;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private readonly SendTransferConnectionInvitationQuery _query = new SendTransferConnectionInvitationQuery();
        private SendTransferConnectionInvitationResponse _response;

        [SetUp]
        public void Arrange()
        {
            
            var fixture = new Fixture();
            _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _configurationProvider.CreateMapper();
            _mediator = new Mock<IMediator>();
            _response = fixture.Create<SendTransferConnectionInvitationResponse>();
            var accountId = 34567;
            var encodingService = new Mock<IEncodingService>();
            encodingService.Setup(x => x.Decode("ABC123", EncodingType.AccountId)).Returns(accountId);
            
            _query.ReceiverAccountPublicHashedId = "ABC123";
            _mediator.Setup(m => m.Send(It.Is<SendTransferConnectionInvitationQuery>(c => c.AccountId.Equals(accountId)
                && c.ReceiverAccountPublicHashedId.Equals("ABC123")), CancellationToken.None)).ReturnsAsync(_response);

            _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object, null, encodingService.Object);
        }

        [Test]
        public async Task ThenAGetCreatedTransferConnectionQueryShouldBeSent()
        {
            await _controller.Send("ABC123",_query);

            _mediator.Verify(m => m.Send(_query, CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeShownTheSendTransferConnectionInvitationPage()
        {
            var result = await _controller.Send("ABC123",_query) as ViewResult;
            var model = result?.Model as SendTransferConnectionInvitationViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(model, Is.Not.Null);
        }
    }
}