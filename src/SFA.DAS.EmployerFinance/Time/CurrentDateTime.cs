﻿using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Time
{
    public sealed class CurrentDateTime : ICurrentDateTime
    {
        public DateTime Now { get; }

        public CurrentDateTime()
        {
            Now = DateTime.UtcNow;
        }

        public CurrentDateTime(DateTime time)
        {
            Now = time;
        }
    }
}