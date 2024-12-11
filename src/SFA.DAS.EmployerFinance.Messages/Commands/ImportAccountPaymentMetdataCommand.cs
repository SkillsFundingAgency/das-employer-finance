using System;

namespace SFA.DAS.EmployerFinance.Messages.Commands
{
    public class ImportAccountPaymentMetadataCommand
    {
        public string PeriodEndRef { get; init; }
        public long AccountId { get; init; }
        public Guid PaymentId { get; init; }
    }
}
