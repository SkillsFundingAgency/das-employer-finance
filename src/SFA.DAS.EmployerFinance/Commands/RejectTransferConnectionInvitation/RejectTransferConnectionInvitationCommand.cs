﻿namespace SFA.DAS.EmployerFinance.Commands.RejectTransferConnectionInvitation;

public class RejectTransferConnectionInvitationCommand :IRequest
{
    public long AccountId { get; set; }

    public Guid UserRef { get; set; }

    public int? TransferConnectionInvitationId { get; set; }
}