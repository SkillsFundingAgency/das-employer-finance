﻿using SFA.DAS.EmployerFinance.Messages.Events;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Models.TransferConnections;

public class TransferConnectionInvitation : Entity
{
    public virtual int Id { get; protected set; }
    public virtual ICollection<TransferConnectionInvitationChange> Changes { get; protected set; } = new List<TransferConnectionInvitationChange>();
    public DateTime CreatedDate { get; protected set; }
    public bool DeletedByReceiver { get; protected set; }
    public bool DeletedBySender { get; protected set; }
    public virtual Account.Account ReceiverAccount { get; protected set; }
    public virtual long ReceiverAccountId { get; protected set; }
    public virtual Account.Account SenderAccount { get; protected set; }
    public virtual long SenderAccountId { get; protected set; }
    public virtual TransferConnectionInvitationStatus Status { get; protected set; }

    public TransferConnectionInvitation(Account.Account senderAccount, Account.Account receiverAccount, User senderUser) : this()
    {
        var now = DateTime.UtcNow;

        SenderAccount = senderAccount;
        ReceiverAccount = receiverAccount;
        Status = TransferConnectionInvitationStatus.Pending;
        CreatedDate = now;

        Changes.Add(new TransferConnectionInvitationChange
        {
            SenderAccount = SenderAccount,
            ReceiverAccount = ReceiverAccount,
            Status = Status,
            DeletedBySender = DeletedBySender,
            DeletedByReceiver = DeletedByReceiver,
            User = senderUser,
            CreatedDate = now
        });

        Publish(() => new SentTransferConnectionRequestEvent
        {
            Created = now,
            ReceiverAccountId = ReceiverAccount.Id,
            ReceiverAccountName = ReceiverAccount.Name,
            SenderAccountId = SenderAccount.Id,
            SenderAccountName = SenderAccount.Name,
            SentByUserId = senderUser.Id,
            SentByUserName = senderUser.FullName,
            SentByUserRef = senderUser.Ref,
            TransferConnectionRequestId = Id
        });
    }

    public TransferConnectionInvitation()
    {
    }

    public void Approve(Account.Account approverAccount, User approverUser)
    {
        RequiresApproverAccountIsTheReceiverAccount(approverAccount);
        RequiresTransferConnectionInvitationIsPending();

        var now = DateTime.UtcNow;

        Status = TransferConnectionInvitationStatus.Approved;

        Changes.Add(new TransferConnectionInvitationChange
        {
            Status = Status,
            User = approverUser,
            CreatedDate = now
        });

        Publish(() => new ApprovedTransferConnectionRequestEvent
        {
            ApprovedByUserId = approverUser.Id,
            ApprovedByUserName = approverUser.FullName,
            ApprovedByUserRef = approverUser.Ref,
            Created = now,
            ReceiverAccountHashedId = ReceiverAccount.HashedId,
            ReceiverAccountId = ReceiverAccount.Id,
            ReceiverAccountName = ReceiverAccount.Name,
            SenderAccountHashedId = SenderAccount.HashedId,
            SenderAccountId = SenderAccount.Id,
            SenderAccountName = SenderAccount.Name,
            TransferConnectionRequestId = Id
        });
    }

    public void Delete(Account.Account deleterAccount, User deleterUser)
    {
        RequiresTransferConnectionInvitationIsRejected();
        RequiresDeleterIsEitherSenderOrReceiver(deleterAccount);

        var now = DateTime.UtcNow;

        bool? deletedBySender = null;
        bool? deletedByReceiver = null;

        if (ReceiverAccountId == deleterAccount.Id)
        {
            RequiresDeleterAccountIsTheReceiverAccount(deleterAccount);
            RequiresNotAlreadyDeletedByReceiver();
            DeletedByReceiver = true;
            deletedByReceiver = true;
        }
        else
        {
            RequiresDeleterAccountIsTheSenderAccount(deleterAccount);
            RequiresNotAlreadyDeletedBySender();
            DeletedBySender = true;
            deletedBySender = true;
        }

        Changes.Add(new TransferConnectionInvitationChange
        {
            DeletedBySender = deletedBySender,
            DeletedByReceiver = deletedByReceiver,
            User = deleterUser,
            CreatedDate = now
        });

        Publish(() => new DeletedTransferConnectionRequestEvent
        {
            Created = now,
            DeletedByAccountId = deleterAccount.Id,
            DeletedByUserId = deleterUser.Id,
            DeletedByUserName = deleterUser.FullName,
            DeletedByUserRef = deleterUser.Ref,
            ReceiverAccountHashedId = ReceiverAccount.HashedId,
            ReceiverAccountId = ReceiverAccount.Id,
            ReceiverAccountName = ReceiverAccount.Name,
            SenderAccountHashedId = SenderAccount.HashedId,
            SenderAccountId = SenderAccount.Id,
            SenderAccountName = SenderAccount.Name,
            TransferConnectionRequestId = Id
        });
    }

    public void Reject(Account.Account rejectorAccount, User rejectorUser)
    {
        RequiresRejectorAccountIsTheReceiverAccount(rejectorAccount);
        RequiresTransferConnectionInvitationIsPending();

        var now = DateTime.UtcNow;

        Status = TransferConnectionInvitationStatus.Rejected;

        Changes.Add(new TransferConnectionInvitationChange
        {
            Status = Status,
            User = rejectorUser,
            CreatedDate = now
        });

        Publish(() => new RejectedTransferConnectionRequestEvent
        {
            Created = now,
            ReceiverAccountHashedId = ReceiverAccount.HashedId,
            ReceiverAccountId = ReceiverAccount.Id,
            ReceiverAccountName = ReceiverAccount.Name,
            RejectorUserId = rejectorUser.Id,
            RejectorUserName = rejectorUser.FullName,
            RejectorUserRef = rejectorUser.Ref,
            SenderAccountHashedId = SenderAccount.HashedId,
            SenderAccountId = SenderAccount.Id,
            SenderAccountName = SenderAccount.Name,
            TransferConnectionRequestId = Id
        });
    }

    private void RequiresApproverAccountIsTheReceiverAccount(Account.Account approverAccount)
    {
        if (approverAccount.Id != ReceiverAccount.Id)
            throw new InvalidOperationException("Requires approver account is the receiver account");
    }

    private void RequiresDeleterAccountIsTheSenderAccount(Account.Account deleterAccount)
    {
        if (deleterAccount.Id != SenderAccount.Id)
            throw new InvalidOperationException("Requires deleter account is the sender account");
    }

    private void RequiresDeleterAccountIsTheReceiverAccount(Account.Account deleterAccount)
    {
        if (deleterAccount.Id != ReceiverAccount.Id)
            throw new InvalidOperationException("Requires deleter account is the receiver account");
    }

    private void RequiresDeleterIsEitherSenderOrReceiver(Account.Account deleterAccount)
    {
        if (deleterAccount.Id != ReceiverAccountId && deleterAccount.Id != SenderAccountId)
        {
            throw new InvalidOperationException("Requires deleter account is either the sender or the receiver");
        }
    }

    private void RequiresNotAlreadyDeletedBySender()
    {
        if (DeletedBySender)
            throw new InvalidOperationException("Requires not already deleted by sender");
    }

    private void RequiresNotAlreadyDeletedByReceiver()
    {
        if (DeletedByReceiver)
            throw new InvalidOperationException("Requires not already deleted by receiver");
    }

    private void RequiresRejectorAccountIsTheReceiverAccount(Account.Account rejectorAccount)
    {
        if (rejectorAccount.Id != ReceiverAccount.Id)
            throw new InvalidOperationException("Requires rejector account is the receiver account");
    }

    private void RequiresTransferConnectionInvitationIsPending()
    {
        if (Status != TransferConnectionInvitationStatus.Pending)
            throw new InvalidOperationException("Requires transfer connection invitation is pending");
    }

    private void RequiresTransferConnectionInvitationIsRejected()
    {
        if (Status != TransferConnectionInvitationStatus.Rejected)
            throw new InvalidOperationException("Requires transfer connection invitation is rejected");
    }
}