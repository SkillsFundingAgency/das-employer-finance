namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity;

public class RemoveAccountLegalEntityCommand : IRequest<Unit>
{
    public RemoveAccountLegalEntityCommand(long id)
    {
        Id = id;
    }

    public long Id { get; set; }
}