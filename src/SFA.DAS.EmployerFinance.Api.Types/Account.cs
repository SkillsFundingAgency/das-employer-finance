using System.Collections.Generic;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class Account
{
    public virtual long Id { get; set; }
    public List<AccountLegalEntity> AccountLegalEntities { get; set; } = new List<AccountLegalEntity>();
    public virtual string Name { get; set; }
    public string HashedId { get; set; }
    public string PublicHashedId { get; set; }
    public List<TransferConnectionInvitation> ReceivedTransferConnectionInvitations { get; set; } = new List<TransferConnectionInvitation>();
    public List<TransferConnectionInvitation> SentTransferConnectionInvitations { get; set; } = new List<TransferConnectionInvitation>();
}