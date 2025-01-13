using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.EmployerFinance.TestCommon.Builders;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetTransferConnectionsTests;

[TestFixture]
public class WhenIGetTransferConnections
{
    private GetTransferConnectionsQueryHandler _handler;
    private GetTransferConnectionsQuery _query;
    private GetTransferConnectionsResponse _response;
    private Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository;
    private IMapper _mapper;
    private TransferConnectionInvitation _approvedTransferConnectionAtoC;
    private TransferConnectionInvitation _approvedTransferConnectionBtoC;
    private Account _accountA;
    private Account _accountB;
    private Account _accountC;

    [SetUp]
    public void Arrange()
    {
        _transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();

        const string hashedAccountId = "ABC123";

        _accountA = new Account
        {
            Id = 111111,
            Name = "Account A",
            HashedId = hashedAccountId
        };

        _accountB = new Account
        {
            Id = 222222,
            Name = "Account B",
            HashedId = hashedAccountId
        };

        _accountC = new Account
        {
            Id = 333333,
            Name = "Account C",
            HashedId = hashedAccountId
        };
    }

    private void SeedDataByStatus(TransferConnectionInvitationStatus status)
    {
        _approvedTransferConnectionAtoC = new TransferConnectionInvitationBuilder()
           .WithId(333333)
           .WithSenderAccount(_accountA)
           .WithReceiverAccount(_accountC)
           .WithStatus(status)
           .Build();

        _approvedTransferConnectionBtoC = new TransferConnectionInvitationBuilder()
            .WithId(444444)
            .WithSenderAccount(_accountB)
            .WithReceiverAccount(_accountC)
            .WithStatus(status)
            .Build();

        _transferConnectionInvitationRepository
            .Setup(s => s.GetByReceiver(_accountC.Id, status))
            .ReturnsAsync([
                _approvedTransferConnectionAtoC,
                _approvedTransferConnectionBtoC
            ]);

        _query = new GetTransferConnectionsQuery
        {
            AccountId = _accountC.Id,
            Status = status
        };

        _transferConnectionInvitationRepository
            .Setup(s => s.GetByReceiver(_accountC.Id, TransferConnectionInvitationStatus.Approved))
            .ReturnsAsync([
                _approvedTransferConnectionAtoC,
                _approvedTransferConnectionBtoC
            ]);

        _mapper = new Mapper(new MapperConfiguration(c =>
        {
            c.AddProfile<AccountMappings>();
            c.AddProfile<TransferConnectionInvitationMappings>();
        }));

        _handler = new GetTransferConnectionsQueryHandler(_transferConnectionInvitationRepository.Object, _mapper);
    }

    [Test]
    public async Task ThenShouldReturnGetTransferConnectionInvitationsResponseForApprovedStatus()
    {
        SeedDataByStatus(TransferConnectionInvitationStatus.Approved);

        _response = await _handler.Handle(_query, CancellationToken.None);

        Assert.That(_response, Is.Not.Null);
        Assert.That(_response.TransferConnections.Count(), Is.EqualTo(2));

        var transferConnectionInvitation = _response.TransferConnections.ElementAt(0);

        Assert.That(transferConnectionInvitation.FundingEmployerAccountId, Is.EqualTo(_accountA.Id));
        Assert.That(transferConnectionInvitation.FundingEmployerAccountName, Is.EqualTo(_accountA.Name));

        var transferConnectionInvitation1 = _response.TransferConnections.ElementAt(1);

        Assert.That(transferConnectionInvitation1.FundingEmployerAccountId, Is.EqualTo(_accountB.Id));
        Assert.That(transferConnectionInvitation1.FundingEmployerAccountName, Is.EqualTo(_accountB.Name));
    }

    [Test]
    public async Task ThenShouldReturnGetTransferConnectionInvitationsResponseForPendingStatus()
    {
        SeedDataByStatus(TransferConnectionInvitationStatus.Pending);

        _response = await _handler.Handle(_query, CancellationToken.None);

        Assert.That(_response, Is.Not.Null);
        Assert.That(_response.TransferConnections.Count(), Is.EqualTo(2));

        var transferConnectionInvitation = _response.TransferConnections.ElementAt(0);

        Assert.That(transferConnectionInvitation.FundingEmployerAccountId, Is.EqualTo(_accountA.Id));
        Assert.That(transferConnectionInvitation.FundingEmployerAccountName, Is.EqualTo(_accountA.Name));

        var transferConnectionInvitation1 = _response.TransferConnections.ElementAt(1);

        Assert.That(transferConnectionInvitation1.FundingEmployerAccountId, Is.EqualTo(_accountB.Id));
        Assert.That(transferConnectionInvitation1.FundingEmployerAccountName, Is.EqualTo(_accountB.Name));
    }
}