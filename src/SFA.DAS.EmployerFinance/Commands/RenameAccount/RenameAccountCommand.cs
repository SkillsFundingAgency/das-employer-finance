using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.RenameAccount
{
    public class RenameAccountCommand : IAuthorizationContextModel, IRequest<Unit>
    {
        public RenameAccountCommand(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; }
        public string Name { get; }
    }
}