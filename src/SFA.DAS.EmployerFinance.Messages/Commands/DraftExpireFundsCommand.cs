using System;

namespace SFA.DAS.EmployerFinance.Messages.Commands;

public class DraftExpireFundsCommand : Message
{
    public DateTime? DateTo { get; set; }
}