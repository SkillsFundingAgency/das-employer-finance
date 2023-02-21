using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccount;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators
{
    public class WhenGettingFinanceIndex
    {
        private const string HashedAccountId = "123ABC";
        private const string ExternalUser = "Test user";
        private const long AccountId = 1234;

        private Mock<IAccountApiClient> _accountApiClient;
        private Mock<IMediator> _mediator;
        private EmployerAccountTransactionsOrchestrator _orchestrator;
        private GetEmployerAccountResponse _response;
        private Mock<ICurrentDateTime> _currentTime;
        private Mock<IEncodingService> _encodingService;

        [SetUp]
        public void Arrange()
        {
            _accountApiClient = new Mock<IAccountApiClient>();
            _mediator = new Mock<IMediator>();
            _currentTime = new Mock<ICurrentDateTime>();
            _encodingService = new Mock<IEncodingService>();

            _response = new GetEmployerAccountResponse
            {
                Account = new Account
                {
                    Id = AccountId,
                    Name = "Test Account"
                }
            };

            _encodingService.Setup(h => h.Decode(HashedAccountId,EncodingType.AccountId)).Returns(AccountId);

            _mediator.Setup(x => x.Send(It.IsAny<GetEmployerAccountHashedQuery>(), CancellationToken.None))
                .ReturnsAsync(_response);

            _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

            _orchestrator = new EmployerAccountTransactionsOrchestrator(_accountApiClient.Object, _mediator.Object, _currentTime.Object, Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>());
        }

        [Test]
        [TestCase("0", false, TestName = "Non-Levy Employer returned")]
        [TestCase("1", true, TestName = "Levy Employer returned")]
        public async Task ThenShouldDetermineIfEmployerIsLevyOrNonLevy(string apprenticeshipEmployerType, bool isLevy)
        {
            //Arrange
            _accountApiClient.Setup(c => c.GetAccount(AccountId))
                .ReturnsAsync(new AccountDetailViewModel
                {
                    ApprenticeshipEmployerType = apprenticeshipEmployerType
                });

            var getAccountFinanceOverviewQuery = new GetAccountFinanceOverviewQuery
            {
                AccountHashedId = HashedAccountId,
                AccountId = AccountId
            };

            _mediator.Setup(m => m.Send(It.IsAny<GetAccountFinanceOverviewQuery>(), CancellationToken.None))
                .ReturnsAsync(new GetAccountFinanceOverviewResponse());

            //Act
            var response = await _orchestrator.Index(getAccountFinanceOverviewQuery);

            //Assert
            response.Should().NotBeNull();
            response.Data.IsLevyEmployer.Should().Be(isLevy);
        }
    }
}
