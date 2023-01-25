using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.PublishGenericEvent
{
    public class PublishGenericEventCommandHandler : IRequestHandler<PublishGenericEventCommand, Unit>
    {
        private readonly IEventsApi _eventsApi;
        private readonly ILog _logger;

        public PublishGenericEventCommandHandler(IEventsApi eventsApi, ILog logger)
        {
            _eventsApi = eventsApi;
            _logger = logger;
        }

        public async Task<Unit> Handle(PublishGenericEventCommand command, CancellationToken cancellationToken)
        {
            _logger.Info($"Publishing Generic event of type {command.Event.Type}");

            await _eventsApi.CreateGenericEvent(command.Event);

            return Unit.Value;
        }
    }
}
