using SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public interface IAuthenticationOrchestrator
    {
        Task SaveIdentityAttributes(string userRef, string email, string firstName, string lastName);
    }

    public class AuthenticationOrchestrator : IAuthenticationOrchestrator
    {
        private readonly IMediator _mediator;

        public AuthenticationOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SaveIdentityAttributes(string userRef, string email, string firstName, string lastName)
        {
            await _mediator.Send(new UpsertRegisteredUserCommand
            {
                EmailAddress = email,
                UserRef = userRef,
                LastName = lastName,
                FirstName = firstName
            });
        }
    }
}
