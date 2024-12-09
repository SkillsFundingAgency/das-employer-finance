using System;

namespace SFA.DAS.EmployerFinance.Api.Types
{
    public class TransferConnection
    {
        public long FundingEmployerAccountId { get; set; }
        public string FundingEmployerHashedAccountId { get; set; }
        public string FundingEmployerPublicHashedAccountId { get; set; }
        public string FundingEmployerAccountName { get; set; }
        public short? Status { get; set; }
        public DateTime? StatusSetOn { get; set; }
    }
}