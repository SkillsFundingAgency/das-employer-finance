
namespace SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;

public class UpdatePayeInformationCommand : IRequest
{
    public string PayeRef { get; set; }
}