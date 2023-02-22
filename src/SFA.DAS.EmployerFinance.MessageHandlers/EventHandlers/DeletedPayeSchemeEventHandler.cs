using MediatR;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerFinance.Commands.RemoveAccountPaye;

namespace SFA.DAS.EmployerFinance.MessageHandlers.EventHandlers;

public class DeletedPayeSchemeEventHandler : IHandleMessages<DeletedPayeSchemeEvent>
{
    private readonly IMediator _mediator;

    public DeletedPayeSchemeEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(DeletedPayeSchemeEvent message, IMessageHandlerContext context)
    {
        await _mediator.Send(new RemoveAccountPayeCommand(message.AccountId, message.PayeRef));
    }
}