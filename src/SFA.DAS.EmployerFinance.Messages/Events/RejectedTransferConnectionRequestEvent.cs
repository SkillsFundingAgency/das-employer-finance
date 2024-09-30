using System;

namespace SFA.DAS.EmployerFinance.Messages.Events;

public class RejectedTransferConnectionRequestEvent : Message
{
    public string ReceiverAccountHashedId { get; set; }
    public long ReceiverAccountId { get; set; }
    public string ReceiverAccountName { get; set; }
    public long RejectorUserId { get; set; }
    public string RejectorUserName { get; set; }
    public Guid RejectorUserRef { get; set; }
    public long SenderAccountId { get; set; }
    public string SenderAccountName { get; set; }
    public int TransferConnectionRequestId { get; set; }
}