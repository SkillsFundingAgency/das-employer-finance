using SFA.DAS.Authorization.ModelBinding;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;

public class UpdatePayeInformationCommand : IAuthorizationContextModel,IRequest<Unit>
{
    public string PayeRef { get; set; }
}