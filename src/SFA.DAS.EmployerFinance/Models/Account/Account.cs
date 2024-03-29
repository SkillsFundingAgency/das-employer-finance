using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Models.Account;

public class Account : Entity
{
    public virtual long Id { get; set; }
    public virtual ICollection<AccountLegalEntity> AccountLegalEntities { get; set; } = new List<AccountLegalEntity>();
    public virtual string Name { get; set; }
    public string HashedId { get; set; }
    public string PublicHashedId { get; set; }
    public virtual ICollection<TransferConnectionInvitation> ReceivedTransferConnectionInvitations { get; set; } = new List<TransferConnectionInvitation>();
    public virtual ICollection<TransferConnectionInvitation> SentTransferConnectionInvitations { get; set; } = new List<TransferConnectionInvitation>();

    public TransferConnectionInvitation SendTransferConnectionInvitation(Account receiverAccount, User senderUser, decimal senderAccountTransferAllowance)
    {
        RequiresTransferConnectionInvitationSenderIsNotTheReceiver(receiverAccount);
        RequiresMinTransferAllowanceIsAvailable(senderAccountTransferAllowance);
        RequiresTransferConnectionInvitationDoesNotExist(receiverAccount);

        var transferConnectionInvitation = new TransferConnectionInvitation(this, receiverAccount, senderUser);

        // TO DO: investigate what this is being done for why is this not adding the invitation
        // to the list of invitation of the account so that it can be saved later by the unit of work
        SentTransferConnectionInvitations.Add(transferConnectionInvitation);

        return transferConnectionInvitation;
    }

    private static void RequiresMinTransferAllowanceIsAvailable(decimal senderAccountTransferAllowance)
    {
        if (senderAccountTransferAllowance < Constants.TransferConnectionInvitations.SenderMinTransferAllowance)
            throw new InvalidOperationException("Required min transfer allowance is available");
    }

    private void RequiresTransferConnectionInvitationDoesNotExist(Account receiverAccount)
    {
        // transfer connection requests for this account where the
        // receiving account is the intended receiver - this is a duplicate check
        var anySentTransferConnectionInvitations = SentTransferConnectionInvitations.Any(i =>
            i.ReceiverAccount.Id == receiverAccount.Id && (
                i.Status == TransferConnectionInvitationStatus.Pending ||
                i.Status == TransferConnectionInvitationStatus.Approved));

        if (anySentTransferConnectionInvitations)
            throw new InvalidOperationException("Required transfer connection invitation does not exist");
    }

    private void RequiresTransferConnectionInvitationSenderIsNotTheReceiver(Account receiverAccount)
    {
        if (receiverAccount.Id == Id)
            throw new InvalidOperationException("Required transfer connection invitation sender is not the receiver");
    }
}