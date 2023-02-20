using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountTransactionSummaryTests
{
    public class WhenIGetAccountTransactionSummary
    {
        private Mock<ITransactionRepository> _repository;
        private Mock<IEncodingService> _encodingService;
        private GetAccountTransactionSummaryRequest _query;
        private GetAccountTransactionSummaryQueryHandler _requestHandler;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<ITransactionRepository>();
            _encodingService = new Mock<IEncodingService>();

            _query = new GetAccountTransactionSummaryRequest { HashedAccountId = "ABC123" };

            _requestHandler = new GetAccountTransactionSummaryQueryHandler(_encodingService.Object, _repository.Object);
        }

        [Test]
        public async Task ThenTheAccountTransactionSummaryIsReturned()
        {
            //Arrange
            var accountId = 1234;
            _encodingService.Setup(x => x.Decode(_query.HashedAccountId, EncodingType.AccountId)).Returns(accountId);
            var expectedTransactionSummary = new List<TransactionSummary>();
            _repository.Setup(x => x.GetAccountTransactionSummary(accountId)).ReturnsAsync(expectedTransactionSummary);

            //Act
            var response = await _requestHandler.Handle(_query, CancellationToken.None);

            //Assert
            response.Data.Should().BeSameAs(expectedTransactionSummary);
        }
    }
}
