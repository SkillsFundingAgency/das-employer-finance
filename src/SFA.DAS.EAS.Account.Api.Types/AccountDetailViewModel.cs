namespace SFA.DAS.EAS.Account.Api.Types
{
    public class AccountDetailViewModel
    {
        public long AccountId { get; set; }
        public string HashedAccountId { get; set; }
        public string PublicHashedAccountId { get; set; }
        public decimal RemainingTransferAllowance { get; set; }
        public decimal StartingTransferAllowance { get; set; }
		public string ApprenticeshipEmployerType { get; set; }
      }
}
