namespace SFA.DAS.EmployerFinance.Messages.Commands
{
    public class ImportAccountLevyDeclarationsCommand
    {
        public ImportAccountLevyDeclarationsCommand(long accountId, string payeRef)
        {
            AccountId = accountId;
            PayeRef = payeRef;
        }

        public long AccountId { get; set; }
        public string PayeRef { get; set; }
    }
}
