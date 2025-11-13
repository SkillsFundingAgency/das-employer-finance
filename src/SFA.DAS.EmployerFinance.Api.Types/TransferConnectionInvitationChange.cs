using System;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class TransferConnectionInvitationChange
{
    public TransferConnectionInvitationStatus Status { get; set; }
    public bool? DeletedBySender { get; set; }
    public bool? DeletedByReceiver { get; set; }
    public DateTime CreatedDate { get; set; }
    public long UserId { get; set; }
    public string UserFullName { get; set; }
}
