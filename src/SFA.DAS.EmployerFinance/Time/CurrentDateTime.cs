using System;
using System.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Time
{
    public sealed class CurrentDateTime : ICurrentDateTime
    {
        public DateTime Now => _now ?? DateTime.Now;

        private readonly DateTime? _now;

        public CurrentDateTime()
        {
            var setting = ConfigurationManager.AppSettings["CurrentTime"];

            if (DateTime.TryParse(setting, out var now))
            {
                _now = now;
            }
        }

        /// <summary>
        /// For testing use only so a reference date can be supplied instead of using DateTime.UtcNow.
        /// </summary>
        /// <param name="referenceDate">Reference date to be used for testing only.</param>
        public CurrentDateTime(DateTime referenceDate)
        {
            _now = referenceDate;
        }
    }
}