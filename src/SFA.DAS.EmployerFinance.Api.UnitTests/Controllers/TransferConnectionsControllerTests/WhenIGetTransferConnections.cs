using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.TransferConnectionsControllerTests
{
    [TestFixture]
    public class WhenIGetTransferConnections
    {
        private TransferConnectionsController _controller;
        private Mock<IMediator> _mediator;
        private Mock<IEncodingService> _encodingService;
        private GetTransferConnectionsResponse _response;
        private IEnumerable<TransferConnection> _transferConnections;
        private readonly string _hashedAccountId = "GF3XWP";
        private readonly string _publicHashedAccountId = "DJ7JL";
        private readonly int _accountId = 123;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            new GetTransferConnectionsQuery();
            _transferConnections = new List<TransferConnection>
            {
                new TransferConnection { FundingEmployerAccountId = _accountId, FundingEmployerAccountName = "ACCOUNT NAME", FundingEmployerHashedAccountId = _hashedAccountId, FundingEmployerPublicHashedAccountId = _publicHashedAccountId }
            };
            _response = new GetTransferConnectionsResponse { TransferConnections = _transferConnections };

            _encodingService = new Mock<IEncodingService>();
            _encodingService.Setup(x => x.Decode(_hashedAccountId,EncodingType.AccountId)).Returns(_accountId);

            _mediator.Setup(
                    m => m.Send(
                        It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(_accountId)),It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            _controller = new TransferConnectionsController(_mediator.Object, _encodingService.Object);
        }

        [Test]
        public async Task ThenGetTransferConnectionsQueryShouldBeSentWithDecodedHashedAccountId()
        {
            await _controller.GetTransferConnections(_hashedAccountId);

            _mediator.Verify(
                m => m.Send(It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(_accountId)),It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ThenGetTransferConnectionsQueryShouldBeSentWithAccountId()
        {
            await _controller.GetTransferConnections(_accountId);

            _mediator.Verify(
                m => m.Send(It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(_accountId)), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnTransferConnectionsForDecodedHashedAccountId()
        {
            var result = await _controller.GetTransferConnections(_hashedAccountId) as OkObjectResult;

            Assert.IsNotNull(result);

            Assert.That(result.Value, Is.SameAs(_transferConnections));
        }

        [Test]
        public async Task ThenShouldReturnTransferConnectionsForAccountId()
        {
            var result = await _controller.GetTransferConnections(_accountId) as OkObjectResult;

            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value, Is.SameAs(_transferConnections));
        }
    }
}