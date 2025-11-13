using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class Account
{
    public long Id { get; protected set; }
    public string Name { get; protected set; }
    public string HashedId { get; protected set; }
    public string PublicHashedId { get; protected set; }
    public string AccountType { get; protected set; }
    public DateTime CreatedDate { get; protected set; }

    public virtual ICollection<TransferConnectionInvitation> SentTransferConnectionInvitations { get; protected set; }
                = new List<TransferConnectionInvitation>();

    public virtual ICollection<TransferConnectionInvitation> ReceivedTransferConnectionInvitations { get; protected set; }
        = new List<TransferConnectionInvitation>();
}