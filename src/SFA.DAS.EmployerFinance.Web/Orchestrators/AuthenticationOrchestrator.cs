using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.UserProfile;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public interface IAuthenticationOrchestrator
    {
        Task SaveIdentityAttributes(string userRef, string email, string firstName, string lastName);
    }

    public class AuthenticationOrchestrator : IAuthenticationOrchestrator
    {
        private readonly IUserRepository _userRepository;
        
        public AuthenticationOrchestrator(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task SaveIdentityAttributes(string userRef, string email, string firstName, string lastName)
        {
            await _userRepository.Upsert(new User
            {
                Ref = new Guid(userRef),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CorrelationId = Guid.NewGuid().ToString()
            });
        }
    }
}
