
namespace SFA.DAS.EmployerFinance.Messages.Commands;

public class CreateAccountPayeCommand(long accountId, string empRef, string name, string aorn) : Message
{
    public long AccountId { get; } = accountId;
    public string EmpRef { get; } = empRef;
    public string Name { get; } = name;
    public string Aorn { get; } = aorn;
}