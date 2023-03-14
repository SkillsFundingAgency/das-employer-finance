using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitations;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransfersControllerTests
{
    [TestFixture]
    public class WhenIViewTheTransferConnectionInvitationsComponent
    {
        private TransferConnectionsController _controller;
        private GetTransferConnectionInvitationsResponse _response;
        private IConfigurationProvider _mapperConfig;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private const long AccountId = 123125;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _response = fixture.Create<GetTransferConnectionInvitationsResponse>();
            
            _mapperConfig = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _mapperConfig.CreateMapper();
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(It.Is<GetTransferConnectionInvitationsQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None)).ReturnsAsync(_response);
            
            _controller = new TransferConnectionsController(null, _mapper, _mediator.Object, Mock.Of<IEncodingService>());
        }

        [Test]
        public async Task ThenAGetTransferConnectionInvitationsQueryShouldBeSent()
        {
            await _controller.TransferConnectionInvitations(AccountId);

            _mediator.Verify(m => m.Send(It.Is<GetTransferConnectionInvitationsQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeShownTheTransferConnectionInvitationsComponent()
        {
            var model = await _controller.TransferConnectionInvitations(AccountId);
            
            Assert.That(model, Is.Not.Null);
        }
    }
}