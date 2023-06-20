
namespace SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;

public class UpdatePayeInformationCommand : IRequest<Unit>
{
    public string PayeRef { get; set; }
}