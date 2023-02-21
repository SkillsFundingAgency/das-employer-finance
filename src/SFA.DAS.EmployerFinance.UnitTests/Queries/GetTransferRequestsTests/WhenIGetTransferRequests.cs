using AutoMapper;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Queries.GetTransferRequests;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.EntityFramework;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransferRequestsTests;

[TestFixture]
public class WhenIGetTransferRequests
{
    private GetTransferRequestsQueryHandler _handler;
    private GetTransferRequestsQuery _query;
    private GetTransferRequestsResponse _response;

    private Mock<IEmployerAccountRepository> _employerAccountsRepository;
    private Mapper _mapper;
    private Mock<ICommitmentsV2ApiClient> _commitmentsV2ApiClient;
    private Mock<IEncodingService> _encodingService;

    private TransferRequestSummaryResponse _sentTransferRequest;
    private TransferRequestSummaryResponse _receivedTransferRequest;
    private GetTransferRequestSummaryResponse _getTransferRequestSummaryResponse;
    private DbSetStub<Account> _accountsDbSet;
    private List<Account> _accounts;
    private Account _account1;
    private Account _account2;

    [SetUp]
    public void Arrange()
    {
        _encodingService = new Mock<IEncodingService>();
        _commitmentsV2ApiClient = new Mock<ICommitmentsV2ApiClient>();
            
        var hashedAccountId = "ABC123";

        _account1 = new Account
        {
            Id = 11111,
            Name = "Account 1",
            HashedId = hashedAccountId
        };

        _account2 = new Account
        {
            Id = 22222,
            Name = "Account 2",
            HashedId = "DEF2564"
        };

        _encodingService.Setup(h => h.Encode(_account1.Id,EncodingType.AccountId)).Returns("ABC123");
        _encodingService.Setup(h => h.Encode(_account2.Id,EncodingType.AccountId)).Returns("DEF456");
        _encodingService.Setup(h => h.Decode("ABC123",EncodingType.AccountId)).Returns(_account1.Id);
        _encodingService.Setup(h => h.Decode("DEF456",EncodingType.AccountId)).Returns(_account2.Id);

        _encodingService.Setup(h => h.Encode(_account1.Id, EncodingType.PublicAccountId)).Returns("123ABC");
        _encodingService.Setup(h => h.Encode(_account2.Id, EncodingType.PublicAccountId)).Returns("456DEF");
        _encodingService.Setup(h => h.Decode("123ABC", EncodingType.PublicAccountId)).Returns(_account1.Id);
        _encodingService.Setup(h => h.Decode("456DEF", EncodingType.PublicAccountId)).Returns(_account2.Id);

        _sentTransferRequest = new TransferRequestSummaryResponse
        {
            HashedTransferRequestId = "GHI789",
            HashedSendingEmployerAccountId = _account1.HashedId,
            HashedReceivingEmployerAccountId = _account2.HashedId,
            TransferCost = 123.456m,
        };

        _receivedTransferRequest = new TransferRequestSummaryResponse
        {
            HashedTransferRequestId = "JKL012",
            HashedSendingEmployerAccountId = _account2.HashedId,
            HashedReceivingEmployerAccountId = _account1.HashedId,
            TransferCost = 789.012m,
        };

        _getTransferRequestSummaryResponse = new GetTransferRequestSummaryResponse
        {
            TransferRequestSummaryResponse = new List<TransferRequestSummaryResponse>
            {
                _sentTransferRequest,
                _receivedTransferRequest
            }
        };

        _accounts = new List<Account>
        {
            _account1,
            _account2
        };

        _mapper = new Mapper(new MapperConfiguration(c =>
        {
            c.AddProfile<AccountMappings>();
        }));

        _employerAccountsRepository = new Mock<IEmployerAccountRepository>();
        _employerAccountsRepository
            .Setup(s => s.Get(It.IsAny<List<long>>()))
            .ReturnsAsync(_accounts);

        _commitmentsV2ApiClient.Setup(c => c.GetTransferRequests(_account1.Id)).ReturnsAsync(_getTransferRequestSummaryResponse);

        _handler = new GetTransferRequestsQueryHandler(_employerAccountsRepository.Object, _mapper, _commitmentsV2ApiClient.Object, _encodingService.Object);

        _query = new GetTransferRequestsQuery
        {
            AccountId = _account1.Id
        };
    }

    [Test]
    public async Task ThenShouldReturnGetTransferRequestsResponse()
    {
        _response = await _handler.Handle(_query, CancellationToken.None);

        Assert.That(_response, Is.Not.Null);
        Assert.That(_response.TransferRequests.Count(), Is.EqualTo(2));

        var sentTransferRequest = _response.TransferRequests.ElementAt(0);

        Assert.That(sentTransferRequest, Is.Not.Null);
        Assert.That(sentTransferRequest.TransferRequestHashedId, Is.EqualTo(_sentTransferRequest.HashedTransferRequestId));
        Assert.That(sentTransferRequest.ReceiverAccount.HashedId, Is.EqualTo(_account2.HashedId));
        Assert.That(sentTransferRequest.SenderAccount.HashedId, Is.EqualTo(_account1.HashedId));
        Assert.That(sentTransferRequest.Status, Is.EqualTo(_sentTransferRequest.Status));
        Assert.That(sentTransferRequest.TransferCost, Is.EqualTo(_sentTransferRequest.TransferCost));

        var receivedTransferRequest = _response.TransferRequests.ElementAt(1);

        Assert.That(receivedTransferRequest, Is.Not.Null);
        Assert.That(receivedTransferRequest.TransferRequestHashedId, Is.EqualTo(_receivedTransferRequest.HashedTransferRequestId));
        Assert.That(receivedTransferRequest.ReceiverAccount.HashedId, Is.EqualTo(_account1.HashedId));
        Assert.That(receivedTransferRequest.SenderAccount.HashedId, Is.EqualTo(_account2.HashedId));
        Assert.That(receivedTransferRequest.Status, Is.EqualTo(_receivedTransferRequest.Status));
        Assert.That(receivedTransferRequest.TransferCost, Is.EqualTo(_receivedTransferRequest.TransferCost));
    }
}