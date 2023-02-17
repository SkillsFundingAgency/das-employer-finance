﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests
{
    public class WhenIGetTransactionsForAnAccount
    {
        private AccountTransactionsController _controller;
        private Mock<IMediator> _mediator;
        private Mock<ILog> _logger;
        private Mock<IUrlHelper> _urlHelper;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILog>();
            _urlHelper = new Mock<IUrlHelper>();
           
            var orchestrator = new AccountTransactionsOrchestrator(_mediator.Object, _logger.Object, _urlHelper.Object);
            _controller = new AccountTransactionsController(orchestrator, _urlHelper.Object);
            _controller.Url = _urlHelper.Object;
        }

        [Test]
        public async Task ThenTheTransactionsAreReturned()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var year = 2017;
            var month = 3;
            var transactionsResponse = new GetEmployerAccountTransactionsResponse
            {                
                Data = TransactionLineObjectMother.Create(),
                AccountHasPreviousTransactions = false
            };
            _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);
           
            //Act
            var response = await _controller.GetTransactions(hashedAccountId, year, month);

            //Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkObjectResult>(response);
            var model = ((OkObjectResult)response).Value as Transactions;

            model?.Should().NotBeNull();
            model?.ShouldAllBeEquivalentTo(transactionsResponse.Data.TransactionLines, options => options.Excluding(x => x.ResourceUri));
        }

        [Test]
        public async Task AndThereAreNoPreviousTransactionThenTheUrlIsNotSet()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var year = 2017;
            var month = 3;           
            var transactionsResponse = new GetEmployerAccountTransactionsResponse
            {
                Data = TransactionLineObjectMother.Create(),
                AccountHasPreviousTransactions = false
            };
            _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

            //Act
            var response = await _controller.GetTransactions(hashedAccountId, year, month);
            
            //Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkObjectResult>(response);
            var model = ((OkObjectResult)response).Value as Transactions;

            model?.Should().NotBeNull();
            model?.PreviousMonthUri.Should().BeNullOrEmpty();

            //_urlHelper.Verify();//.Verify(x => x.RouteUrl("GetTransactions", It.IsAny<object>()), Times.Never);

            _urlHelper.Setup(x => x.RouteUrl(
              It.Is<UrlRouteContext>(c =>
              c.RouteName == "GetTransactions")));
        }

        [Test]
        public async Task AndThereArePreviousTransactionsThenTheLinkIsCorrect()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var year = 2017;
            var month = 1;            
            var transactionsResponse = new GetEmployerAccountTransactionsResponse
            {
                Data = TransactionLineObjectMother.Create(),
                AccountHasPreviousTransactions = true,
                Year = year,
                Month = month
            };
            _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

            //Act
            var expectedUri = "someuri";

            _urlHelper.Setup(x => x.RouteUrl(
                It.Is<UrlRouteContext>(c =>
                c.RouteName == "GetTransactions" && c.Values.IsEquivalentTo(new { hashedAccountId, year = year - 1, month = 12 }))))
            .Returns(expectedUri);

            //Assert
            var response = await _controller.GetTransactions(hashedAccountId, year, month);
            var model = ((OkObjectResult)response).Value as Transactions;

            model?.PreviousMonthUri.Should().Be(expectedUri);
        }

        [Test]
        public async Task AndNoMonthIsProvidedThenTheCurrentMonthIsUsed()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var year = 2017;           
            var transactionsResponse = new GetEmployerAccountTransactionsResponse
            {
                Data = TransactionLineObjectMother.Create(),
                AccountHasPreviousTransactions = false
            };
            _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == DateTime.Now.Month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

            //Act
            var response = await _controller.GetTransactions(hashedAccountId, year);

            //Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkObjectResult>(response);
            var model = ((OkObjectResult)response).Value as Transactions;

            model?.Should().NotBeNull();
            model?.ShouldAllBeEquivalentTo(transactionsResponse.Data.TransactionLines, options => options.Excluding(x => x.ResourceUri));
            model?.PreviousMonthUri.Should().BeNullOrEmpty();

            //_urlHelper.Verify(x => x.RouteUrl("GetTransactions", It.IsAny<object>()));

            _urlHelper.Setup(x => x.RouteUrl(
             It.Is<UrlRouteContext>(c =>
             c.RouteName == "GetTransactions")));
        }


        [Test]
        public async Task AndThereAreLevyTransactionsThenTheLinkIsCorrect()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var year = 2017;
            var month = 1;
            var levyTransaction = TransactionLineObjectMother.Create();
            var transactionsResponse = new GetEmployerAccountTransactionsResponse
            {
                Data = TransactionLineObjectMother.Create(),
                AccountHasPreviousTransactions = false,
                Year = year,
                Month = month
            };
            _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

            var expectedUri = "someuri";

            _urlHelper.Setup(x =>x.RouteUrl(
                            It.Is<UrlRouteContext>(c =>
                            c.RouteName == "GetLevyForPeriod" &&
                            c.Values.IsEquivalentTo(new { hashedAccountId = hashedAccountId, payrollYear = levyTransaction.TransactionLines[0].PayrollYear, payrollMonth = levyTransaction.TransactionLines[0].PayrollMonth }))))
                .Returns(expectedUri);

            //Act
            var response = await _controller.GetTransactions(hashedAccountId, year, month);
            var model = ((OkObjectResult)response).Value as Transactions;

            //Assert            
            model? [0].ResourceUri.Should().Be(expectedUri);
        }


        [Test]
        public async Task AndNoYearIsProvidedThenTheCurrentYearIsUsed()
        {
            //Arrange
            var hashedAccountId = "ABC123";           
            var transactionsResponse = new GetEmployerAccountTransactionsResponse
            {
                Data = TransactionLineObjectMother.Create(),
                AccountHasPreviousTransactions = false
            };
            _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == DateTime.Now.Year && q.Month == DateTime.Now.Month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

            //Act
            var response = await _controller.GetTransactions(hashedAccountId);

            //Assert            
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkObjectResult>(response);
            var model = ((OkObjectResult)response).Value as Transactions;

            model?.Should().NotBeNull();
            model?.ShouldAllBeEquivalentTo(transactionsResponse.Data.TransactionLines, options => options.Excluding(x => x.ResourceUri));
        }
    }    
}
