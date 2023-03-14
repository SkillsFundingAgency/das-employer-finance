﻿using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitationAuthorization;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransfersControllerTests
{
    [TestFixture]
    public class WhenIViewTheTransferConnectionInvitationAuthorizationComponent
    {
        private TransferConnectionsController _controller;
        private GetTransferConnectionInvitationAuthorizationResponse _response;
        private IConfigurationProvider _mapperConfig;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private const long AccountId = 123125;

        [SetUp]
        public void Arrange()
        {
            // var authResult = new AuthorizationResult();
            // authResult.AddError(new EmployerFeatureAgreementNotSigned());
            _response = new GetTransferConnectionInvitationAuthorizationResponse
            {
                //AuthorizationResult = authResult, 
                IsValidSender = true,TransferAllowancePercentage = .25m
            };
            _mapperConfig = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _mapperConfig.CreateMapper();
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(It.Is<GetTransferConnectionInvitationAuthorizationQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None)).ReturnsAsync(_response);

            _controller = new TransferConnectionsController(null, _mapper, _mediator.Object, Mock.Of<IEncodingService>());
        }

        [Test]
        public async Task ThenAGetTransferConnectionInvitationAuthorizationQueryShouldBeSent()
        {
            await _controller.TransferConnectionInvitationAuthorization(AccountId);

            _mediator.Verify(m => m.Send(It.Is<GetTransferConnectionInvitationAuthorizationQuery>(c=>c.AccountId.Equals(AccountId)), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeShownTheTransferConnectionInvitationAuthorizationComponent()
        {
            var model = await _controller.TransferConnectionInvitationAuthorization(AccountId);
            
            Assert.That(model, Is.Not.Null);
            //Assert.That(model.AuthorizationResult, Is.EqualTo(_response.AuthorizationResult)); TODO
            Assert.That(model.IsValidSender, Is.EqualTo(_response.IsValidSender));
        }

        [Test]
        public async Task ThenIShouldBeShownTheCorrectTransferAllowancePercentage()
        {
            //Act
            var model = await _controller.TransferConnectionInvitationAuthorization(AccountId);
            

            //Assert
            Assert.AreEqual(25m, model.TransferAllowancePercentage);
        }
    }
}