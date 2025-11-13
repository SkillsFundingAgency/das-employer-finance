using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFinance.Api.Types;

public class TransferConnectionInvitation
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool DeletedByReceiver { get; set; }
    public bool DeletedBySender { get; set; }
    public long ReceiverAccountId { get; set; }
    public string ReceiverAccountName { get; set; }
    public long SenderAccountId { get; set; }
    public string SenderAccountName { get; set; }
    public TransferConnectionInvitationStatus Status { get; set; }

    public List<TransferConnectionInvitationChange> Changes { get; set; }
}
