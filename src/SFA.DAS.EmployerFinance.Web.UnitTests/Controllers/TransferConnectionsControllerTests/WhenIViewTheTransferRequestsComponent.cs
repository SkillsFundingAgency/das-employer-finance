﻿using System;
using System.Collections.Generic;
using System.Threading;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.EmployerFeatures.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.EmployerFinance.Dtos;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransfersControllerTests
{
    [TestFixture]
    public class WhenIViewTheTransferRequestsComponent
    {
        private TransferConnectionsController _controller;
        private GetTransferRequestsQuery _query;
        private GetTransferRequestsResponse _response;
        private Mock<ILog> _logger;
        private IConfigurationProvider _mapperConfig;
        private IMapper _mapper;
        private Mock<IMediator> _mediator;
        private Mock<IFeatureTogglesService<EmployerFeatureToggle>> _featureToggleService;

        [SetUp]
        public void Arrange()
        {
            _query = new GetTransferRequestsQuery();

            _response = new GetTransferRequestsResponse
            {
                TransferRequests = new List<TransferRequestDto>()
            };

            _logger = new Mock<ILog>();
            _mapperConfig = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
            _mapper = _mapperConfig.CreateMapper();
            _mediator = new Mock<IMediator>();
            _mediator.Setup(m => m.Send(_query, CancellationToken.None)).ReturnsAsync(_response);
            _featureToggleService = new Mock<IFeatureTogglesService<EmployerFeatureToggle>>();

            _controller = new TransferConnectionsController(_logger.Object, _mapper, _mediator.Object, _featureToggleService.Object);
        }

        [Test]
        public void ThenAGetTransferRequestsQueryShouldBeSent()
        {
            _controller.TransferRequests(_query);

            _mediator.Verify(m => m.Send(_query, CancellationToken.None), Times.Once);
        }

        [Test]
        public void ThenIShouldBeShownTheTransferRequestsComponent()
        {
            var result = _controller.TransferRequests(_query) as PartialViewResult;
            var model = result?.Model as TransferRequestsViewModel;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.Null);
            Assert.That(model, Is.Not.Null);
            Assert.That(model.TransferRequests, Is.EqualTo(_response.TransferRequests));
        }

        [Test]
        public void ThenExceptionShouldBeLoggedWhenExceptionIsThrown()
        {
            _mediator.Setup(m => m.Send(_query, CancellationToken.None)).Throws<Exception>();

            var result = _controller.TransferRequests(_query) as EmptyResult;

            Assert.That(result, Is.Not.Null);

            _logger.Verify(l => l.Warn(It.IsAny<Exception>(), "Failed to get transfer requests"), Times.Once);
        }
    }
}