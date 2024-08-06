using System;
using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerFinance.Messages.Commands
{
    public class ImportAccountPaymentMetadataCommand : Command
    {
        public string PeriodEndRef { get; init; }
        public long AccountId { get; init; }
        public Guid PaymentId { get; init; }
    }
}
