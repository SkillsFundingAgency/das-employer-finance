﻿using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;
using SFA.DAS.EmployerFinance.TestCommon.Builders;
using SFA.DAS.Testing;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.EmployerFinance.UnitTests.Models;

[TestFixture]
public class TransferConnectionInvitationTests : FluentTest<TransferConnectionInvitationTestsFixture>
{
    [Test]
    public void SendTransferConnectionInvitation_WhenISendATransferConnection_ThenShouldCreateTransferConnectionInvitation()
    {
        Test(f => f.SendTransferConnectionInvitation(), f => f.TransferConnectionInvitation.Should().NotBeNull());
    }

    [Test]
    public void SendTransferConnectionInvitation_WhenISendATransferConnection_ThenShouldCreateTransferConnectionInvitationChange()
    {
        Test(f => f.SendTransferConnectionInvitation(), f => f.TransferConnectionInvitation.Changes.SingleOrDefault().Should().NotBeNull()
            .And.Match<TransferConnectionInvitationChange>(c =>
                c.ReceiverAccount == f.ReceiverAccount &&
                c.SenderAccount == f.SenderAccount &&
                c.User == f.SenderUser));
    }

    [Test]
    public void SendTransferConnectionInvitation_WhenISendATransferConnection_ThenShouldPublishSentTransferConnectionInvitationEvent()
    {
        Test(f => f.SendTransferConnectionInvitation(), f => f.GetEvent<SentTransferConnectionRequestEvent>().Should().NotBeNull()
            .And.Match<SentTransferConnectionRequestEvent>(e =>
                e.ReceiverAccountId == f.ReceiverAccount.Id &&
                e.ReceiverAccountName == f.ReceiverAccount.Name &&
                e.SenderAccountId == f.SenderAccount.Id &&
                e.SenderAccountName == f.SenderAccount.Name &&
                e.SentByUserRef == f.SenderUser.Ref &&
                e.SentByUserId == f.SenderUser.Id &&
                e.SentByUserName == f.SenderUser.FullName &&
                e.TransferConnectionRequestId == f.TransferConnectionInvitation.Id));
    }
}

public class TransferConnectionInvitationTestsFixture 
{
    public IUnitOfWorkContext UnitOfWorkContext { get; set; } = new UnitOfWorkContext();
    public TransferConnectionInvitation TransferConnectionInvitation { get; set; }
    public Account ReceiverAccount { get; set; }
    public long? Result { get; set; }
    public Account SenderAccount { get; set; }
    public decimal SenderAccountTransferAllowance { get; set; }
    public User SenderUser { get; set; }

    public TransferConnectionInvitationTestsFixture()
    {
        SetSenderAccount()
            .SetReceiverAccount()
            .SetSenderUser()
            .SetSenderAccountTransferAllowance(1);
    }

    public TransferConnectionInvitationTestsFixture AddInvitationFromSenderToReceiver(TransferConnectionInvitationStatus status)
    {
        SenderAccount.SentTransferConnectionInvitations.Add(
            new TransferConnectionInvitationBuilder()
                .WithReceiverAccount(ReceiverAccount)
                .WithStatus(status)
                .Build());

        return this;
    }

    public TransferConnectionInvitationChange GetChange(int index)
    {
        return TransferConnectionInvitation.Changes.ElementAt(index);
    }

    public T GetEvent<T>()
    {
        return UnitOfWorkContext.GetEvents().OfType<T>().SingleOrDefault();
    }

    public void SendTransferConnectionInvitation()
    {
        TransferConnectionInvitation = SenderAccount.SendTransferConnectionInvitation(ReceiverAccount, SenderUser, SenderAccountTransferAllowance);
    }

    public TransferConnectionInvitationTestsFixture SetSenderUser()
    {
        SenderUser = new User
        {
            Ref = Guid.NewGuid(),
            Id = 123456,
            FirstName = "John",
            LastName = "Doe"
        };

        return this;
    }

    public TransferConnectionInvitationTestsFixture SetReceiverAccount()
    {
        var hashedAccountId = "ABC123";
        ReceiverAccount = new Account
        {
            Id = 222222,
            Name = "Receiver",
            HashedId = hashedAccountId
        };

        return this;
    }

    public TransferConnectionInvitationTestsFixture SetSenderAccount()
    {
        var hashedAccountId = "ABC123";
        SenderAccount = new Account
        {
            Id = 333333,
            Name = "Sender",
            HashedId = hashedAccountId
        };

        return this;
    }

    public TransferConnectionInvitationTestsFixture SetSenderAccountTransferAllowance(decimal transferAllowance)
    {
        SenderAccountTransferAllowance = transferAllowance;

        return this;
    }
}