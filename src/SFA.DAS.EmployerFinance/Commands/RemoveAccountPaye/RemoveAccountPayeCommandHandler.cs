using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountPaye;

public class RemoveAccountPayeCommandHandler : IRequestHandler<RemoveAccountPayeCommand,Unit>
{
    private readonly IPayeRepository _payeRepository;
    private readonly ILogger<RemoveAccountPayeCommandHandler> _logger;

    public RemoveAccountPayeCommandHandler(IPayeRepository payeRepository, ILogger<RemoveAccountPayeCommandHandler> logger)
    {
        _payeRepository = payeRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveAccountPayeCommand request,CancellationToken cancellationToken)
    {
        try
        {
            await _payeRepository.RemovePayeScheme(request.AccountId, request.PayeRef);

            _logger.LogInformation($"Paye scheme removed - account id: {request.AccountId}; paye ref: {request.PayeRef}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not remove account paye scheme");
            throw;
        }

        return Unit.Value;
    }
}