using System;

namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class RefreshEmployerLevyDataCompletedEvent
    {
        public long AccountId { get; set; }
        public string PayeRef { get; set; }
        public DateTime? LastLevyDeclarationDate { get; set; }
        public short PeriodMonth { get; set; }
        public string PeriodYear { get; set; }
        public bool LevyImported { get; set; }
        public decimal LevyTransactionValue { get; set; }
        public DateTime Created { get; set; }
    }
}
