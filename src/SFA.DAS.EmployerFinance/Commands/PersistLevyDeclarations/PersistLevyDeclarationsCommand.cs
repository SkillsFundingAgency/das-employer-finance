using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;

public class PersistLevyDeclarationsCommand : IRequest<PersistLevyDeclarationsResponse>
{
    public PersistLevyDeclarationRequestData Data { get; set; } = new();
}
