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

        var hashedAccountId = "ABC123";

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
            .ReturnsAsync(new List<TransferConnectionInvitation>
            {
                _approvedTransferConnectionAtoC,
                _approvedTransferConnectionBtoC
            });

        _query = new GetTransferConnectionsQuery
        {
            AccountId = _accountC.Id,
            Status = status
        };

        _transferConnectionInvitationRepository
            .Setup(s => s.GetByReceiver(_accountC.Id, TransferConnectionInvitationStatus.Approved))
            .ReturnsAsync(new List<TransferConnectionInvitation>
            {
                _approvedTransferConnectionAtoC,
                _approvedTransferConnectionBtoC
            });

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

        _response.Should().NotBeNull();
        _response.TransferConnections.Count().Should().Be(2);

        var transferConnectionInvitation = _response.TransferConnections.ElementAt(0);

        transferConnectionInvitation.FundingEmployerAccountId.Should().Be(_accountA.Id);
        transferConnectionInvitation.FundingEmployerHashedAccountId.Should().Be(_accountA.HashedId);
        transferConnectionInvitation.FundingEmployerPublicHashedAccountId.Should().Be(_accountA.PublicHashedId);
        transferConnectionInvitation.FundingEmployerAccountName.Should().Be(_accountA.Name);

        var transferConnectionInvitation1 = _response.TransferConnections.ElementAt(1);

        transferConnectionInvitation1.FundingEmployerAccountId.Should().Be(_accountB.Id);
        transferConnectionInvitation1.FundingEmployerHashedAccountId.Should().Be(_accountB.HashedId);
        transferConnectionInvitation1.FundingEmployerPublicHashedAccountId.Should().Be(_accountB.PublicHashedId);
        transferConnectionInvitation1.FundingEmployerAccountName.Should().Be(_accountB.Name);
        transferConnectionInvitation1.Status.Should().Be((short?)TransferConnectionInvitationStatus.Approved);
    }

    [Test]
    public async Task ThenShouldReturnGetTransferConnectionInvitationsResponseForPendingStatus()
    {
        SeedDataByStatus(TransferConnectionInvitationStatus.Pending);

        _response = await _handler.Handle(_query, CancellationToken.None);

        _response.Should().NotBeNull();
        _response.TransferConnections.Count().Should().Be(2);

        var transferConnectionInvitation = _response.TransferConnections.ElementAt(0);

        transferConnectionInvitation.FundingEmployerAccountId.Should().Be(_accountA.Id);
        transferConnectionInvitation.FundingEmployerHashedAccountId.Should().Be(_accountA.HashedId);
        transferConnectionInvitation.FundingEmployerPublicHashedAccountId.Should().Be(_accountA.PublicHashedId);
        transferConnectionInvitation.FundingEmployerAccountName.Should().Be(_accountA.Name);

        var transferConnectionInvitation1 = _response.TransferConnections.ElementAt(1);

        transferConnectionInvitation1.FundingEmployerAccountId.Should().Be(_accountB.Id);
        transferConnectionInvitation1.FundingEmployerHashedAccountId.Should().Be(_accountB.HashedId);
        transferConnectionInvitation1.FundingEmployerPublicHashedAccountId.Should().Be(_accountB.PublicHashedId);
        transferConnectionInvitation1.FundingEmployerAccountName.Should().Be(_accountB.Name);
    }
}