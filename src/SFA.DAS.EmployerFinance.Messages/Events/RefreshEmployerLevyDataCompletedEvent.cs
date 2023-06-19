﻿using System;

namespace SFA.DAS.EmployerFinance.Messages.Events
{
    public class RefreshEmployerLevyDataCompletedEvent
    {
        public long AccountId { get; set; }
        public short PeriodMonth { get; set; }
        public string PeriodYear { get; set; }
        /// <summary>
        /// true if we have imported some levy; otherwise false;
        /// </summary>
        public bool LevyImported { get; set; }
        public decimal LevyTransactionValue { get; set; }
        public DateTime Created { get; set; }
    }
}
