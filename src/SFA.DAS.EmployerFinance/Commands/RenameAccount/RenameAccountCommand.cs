
namespace SFA.DAS.EmployerFinance.Commands.RenameAccount;

public class RenameAccountCommand : IRequest<Unit>
{
    public RenameAccountCommand(long id, string name)
    {
        Id = id;
        Name = name;
    }

    public long Id { get; }
    public string Name { get; }
}