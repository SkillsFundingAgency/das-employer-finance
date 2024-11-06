namespace SFA.DAS.EmployerFinance.Messages.Commands
{
    public class CreateAccountPayeCommand
    {
        public CreateAccountPayeCommand(long accountId, string empRef, string name, string aorn)
        {
            AccountId = accountId;
            EmpRef = empRef;
            Name = name;
            Aorn = aorn;
        }

        public long AccountId { get; }
        public string EmpRef { get; }
        public string Name { get; }
        public string Aorn { get; }
    }
}