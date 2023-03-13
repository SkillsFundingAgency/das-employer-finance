using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.EmployerFinance.Commands.PublishGenericEvent;

public class PublishGenericEventCommand : IRequest<Unit>
{
    public GenericEvent Event { get; set; }
}