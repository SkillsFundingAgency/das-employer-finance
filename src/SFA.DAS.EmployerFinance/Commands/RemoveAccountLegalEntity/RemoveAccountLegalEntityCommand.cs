using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountLegalEntity;

public class RemoveAccountLegalEntityCommand : IAuthorizationContextModel,IRequest<Unit>
{
    public RemoveAccountLegalEntityCommand(long id)
    {
        Id = id;
    }

    public long Id { get; set; }
}