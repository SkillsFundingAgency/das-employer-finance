using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests
{
    [TestFixture]
    public class WhenIGetTheTransactionSummaryForAnAccount
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
            var orchestrator = new AccountTransactionsOrchestrator(_mediator.Object, _logger.Object);
            _controller = new AccountTransactionsController(orchestrator);
            _controller.Url = _urlHelper.Object;
        }

        [Test]
        public async Task ThenTheTransactionSummaryIsReturned()
        {
            //Arrange
            var hashedAccountId = "ABC123";
            var transactionSummaryResponse = new GetAccountTransactionSummaryResponse
            {
                Data = new List<TransactionSummary> { new TransactionSummary { Month = 1, Year = 2017 }, new TransactionSummary { Month = 2, Year = 2017 } }
            };
            _mediator.Setup(x => x.Send(It.Is<GetAccountTransactionSummaryRequest>(q => q.HashedAccountId == hashedAccountId),It.IsAny<CancellationToken>())).ReturnsAsync(transactionSummaryResponse);


            var firstExpectedUri = "someuri";
            _urlHelper.Setup(
                    x =>
                        x.RouteUrl("GetTransactions",
                            It.Is<object>(o => o.IsEquivalentTo(new { hashedAccountId, year = transactionSummaryResponse.Data.First().Year, month = transactionSummaryResponse.Data.First().Month }))))
                .Returns(firstExpectedUri);
            var secondExpectedUri = "someotheruri";
            _urlHelper.Setup(
                    x =>
                        x.RouteUrl("GetTransactions",
                            It.Is<object>(o => o.IsEquivalentTo(new { hashedAccountId, year = transactionSummaryResponse.Data.Last().Year, month = transactionSummaryResponse.Data.Last().Month }))))
                .Returns(secondExpectedUri);

            //Act
            var response = await _controller.Index(hashedAccountId);

            //Assert            
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<List<TransactionSummary>>>(response);
            var model = response as OkNegotiatedContentResult<List<TransactionSummary>>;

            model?.Content.Should().NotBeNull();
            model?.Content.ShouldAllBeEquivalentTo(transactionSummaryResponse.Data, x => x.Excluding(y => y.Href));
            model?.Content.First().Href.Should().Be(firstExpectedUri);
            model?.Content.Last().Href.Should().Be(secondExpectedUri);
        }      
    }
}
