namespace SFA.DAS.EmployerFinance.Commands.WithdrawTransferConnectionInvitationBySender;

public class WithdrawTransferConnectionInvitationBySenderCommand : IRequest
{
    public int TransferConnectionInvitationId { get; set; }

    public long SenderAccountId { get; set; }
}
