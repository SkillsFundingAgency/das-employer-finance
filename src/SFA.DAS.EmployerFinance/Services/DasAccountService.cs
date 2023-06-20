using SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class DasAccountService : IDasAccountService
{
    private readonly IMediator _mediator;

    public DasAccountService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task UpdatePayeScheme(string empRef)
    {
        await _mediator.Send(new UpdatePayeInformationCommand {PayeRef = empRef});
    }
}