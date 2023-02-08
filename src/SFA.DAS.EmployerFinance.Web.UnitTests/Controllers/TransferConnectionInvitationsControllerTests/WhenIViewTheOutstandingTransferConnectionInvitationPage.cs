﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetLatestPendingReceivedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenIViewTheOutstandingTransferConnectionInvitationPage
    {
        private TransferConnectionInvitationsController _controller;
        private IConfigurationProvider _configurationProvider;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void SetUp()
        {
            _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _configurationProvider.CreateMapper();
            _mediator = new Mock<IMediator>();
            _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object, null);
        }

        [Test]
        public async Task ThenShouldRedirectWhenHasOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = new TransferConnectionInvitationDto()
                });

            var actionResult = await _controller.Outstanding(new GetLatestPendingReceivedTransferConnectionInvitationQuery());

            Assert.AreEqual(typeof(RedirectToRouteResult), actionResult.GetType());
        }

        [Test]
        public async Task ThenShouldRedirectToExpectedRouteWhenHasOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = new TransferConnectionInvitationDto()
                });

            var actionResult = await _controller.Outstanding(new GetLatestPendingReceivedTransferConnectionInvitationQuery()) as RedirectToRouteResult;

            CheckRoute(
                null,
                nameof(TransferConnectionInvitationsController.Receive),
                actionResult);
        }

        [Test]
        public async Task ThenShouldRedirectWhenDoesNotHaveOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = null
                });

            var actionResult = await _controller.Outstanding(new GetLatestPendingReceivedTransferConnectionInvitationQuery());

            Assert.AreEqual(typeof(RedirectToRouteResult), actionResult.GetType());
        }

        [Test]
        public async Task ThenShouldRedirectToExpectedRouteWhenDoesNotHaveOutstandingTransfer()
        {
            _mediator.Setup(m => m.Send(It.IsAny<IRequest<GetLatestPendingReceivedTransferConnectionInvitationResponse>>(), CancellationToken.None))
                .ReturnsAsync(new GetLatestPendingReceivedTransferConnectionInvitationResponse
                {
                    TransferConnectionInvitation = null
                });

            var actionResult = await _controller.Outstanding(new GetLatestPendingReceivedTransferConnectionInvitationQuery()) as RedirectToRouteResult;

            CheckRoute(
                nameof(TransferConnectionsController),
                nameof(TransferConnectionInvitationsController.Index),
                actionResult);
        }

        private void CheckRoute(string expectedControllerName, string expectedActionName, RedirectToRouteResult actualRoute)
        {
            if (!string.IsNullOrWhiteSpace(expectedControllerName) && expectedControllerName.EndsWith("Controller", StringComparison.InvariantCultureIgnoreCase))
            {
                expectedControllerName = expectedControllerName.Substring(0, expectedControllerName.Length - "Controller".Length);
            }

            Assert.AreEqual(expectedControllerName, actualRoute.RouteValues["Controller"]);
            Assert.AreEqual(expectedActionName, actualRoute.RouteValues["Action"]);
        }
    }
}