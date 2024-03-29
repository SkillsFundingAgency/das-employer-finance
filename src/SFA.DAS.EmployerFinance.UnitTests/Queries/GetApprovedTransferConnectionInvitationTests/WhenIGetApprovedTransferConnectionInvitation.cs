﻿using AutoMapper;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Mappings;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetApprovedTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.TestCommon.Builders;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetApprovedTransferConnectionInvitationTests;

[TestFixture]
public class WhenIGetApprovedTransferConnectionInvitation
{
    private GetApprovedTransferConnectionInvitationQueryHandler _handler;
    private GetApprovedTransferConnectionInvitationQuery _query;
        
    private Mock<ITransferConnectionInvitationRepository> _transferConnectionInvitationRepository;
    private Mapper _mapper;
        
    private TransferConnectionInvitation _approvedTransferConnectionInvitation;
    private Account _senderAccount;
    private Account _receiverAccount;

    [SetUp]
    public void Arrange()
    {
        var hashedAccountId = "ABC123";

        _senderAccount = new Account
        {
            Id = 444444,
            Name = "Sender",
            HashedId = hashedAccountId
        };

        _receiverAccount = new Account
        {
            Id = 333333,
            Name = "Receiver",
            HashedId = hashedAccountId
        };

        _approvedTransferConnectionInvitation = new TransferConnectionInvitationBuilder()
            .WithId(111111)
            .WithSenderAccount(_senderAccount)
            .WithReceiverAccount(_receiverAccount)
            .WithStatus(TransferConnectionInvitationStatus.Approved)
            .Build();

        _mapper = new Mapper(new MapperConfiguration(c =>
        {
            c.AddProfile<AccountMappings>();
            c.AddProfile<TransferConnectionInvitationMappings>();
        }));

        _transferConnectionInvitationRepository = new Mock<ITransferConnectionInvitationRepository>();
        _transferConnectionInvitationRepository
            .Setup(s => s.GetByReceiver(_approvedTransferConnectionInvitation.Id, _receiverAccount.Id, TransferConnectionInvitationStatus.Approved))
            .ReturnsAsync(_approvedTransferConnectionInvitation);

        _handler = new GetApprovedTransferConnectionInvitationQueryHandler(_transferConnectionInvitationRepository.Object, _mapper);
    }

    [Test]
    public async Task ThenShouldReturnApprovedTransferConnectionInvitation()
    {
        _query = new GetApprovedTransferConnectionInvitationQuery
        {
            AccountId = _receiverAccount.Id,
            TransferConnectionInvitationId = _approvedTransferConnectionInvitation.Id
        };

        var response = await _handler.Handle(_query, CancellationToken.None);

        Assert.That(response, Is.Not.Null);
        Assert.That(response, Is.TypeOf<GetApprovedTransferConnectionInvitationResponse>());
        Assert.That(response.TransferConnectionInvitation, Is.Not.Null);
        Assert.That(response.TransferConnectionInvitation.Id, Is.EqualTo(_approvedTransferConnectionInvitation.Id));
    }
}