using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.UpsertRegisteredUser;

public class UpsertRegisteredUserCommand : IAuthorizationContextModel,IRequest<Unit>
{
    public string UserRef { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string CorrelationId { get; set; }
}