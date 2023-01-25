using MediatR;
using SFA.DAS.Authorization.ModelBinding;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.EmployerFinance.Commands.PublishGenericEvent
{
    public class PublishGenericEventCommand : IAuthorizationContextModel,IRequest<Unit>
    {
        public GenericEvent Event { get; set; }
    }
}
