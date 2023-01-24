using MediatR;
using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.CreateAccount
{
    public class CreateAccountCommand : IAuthorizationContextModel, IRequest<Unit>
    {
        public CreateAccountCommand(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public long Id { get; }
        public string Name { get; }
    }
}