﻿using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.HashingService;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountTransactionSummaryTests
{
    public class WhenIGetAccountTransactionSummary
    {
        private Mock<ITransactionRepository> _repository;
        private Mock<IHashingService> _hashingService;
        private GetAccountTransactionSummaryRequest _query;
        private GetAccountTransactionSummaryQueryHandler _requestHandler;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<ITransactionRepository>();
            _hashingService = new Mock<IHashingService>();

            _query = new GetAccountTransactionSummaryRequest { HashedAccountId = "ABC123" };

            _requestHandler = new GetAccountTransactionSummaryQueryHandler(_hashingService.Object, _repository.Object);
        }

        [Test]
        public async Task ThenTheAccountTransactionSummaryIsReturned()
        {
            //Arrange
            var accountId = 1234;
            _hashingService.Setup(x => x.DecodeValue(_query.HashedAccountId)).Returns(accountId);
            var expectedTransactionSummary = new List<TransactionSummary>();
            _repository.Setup(x => x.GetAccountTransactionSummary(accountId)).ReturnsAsync(expectedTransactionSummary);

            //Act
            var response = await _requestHandler.Handle(_query, CancellationToken.None);

            //Assert
            response.Data.Should().BeSameAs(expectedTransactionSummary);
        }
    }
}
