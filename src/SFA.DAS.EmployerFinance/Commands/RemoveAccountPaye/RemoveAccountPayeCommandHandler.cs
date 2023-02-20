using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.Commands.RemoveAccountPaye
{
    public class RemoveAccountPayeCommandHandler : IRequestHandler<RemoveAccountPayeCommand,Unit>
    {
        private readonly IPayeRepository _payeRepository;
        private readonly ILog _logger;

        public RemoveAccountPayeCommandHandler(IPayeRepository payeRepository, ILog logger)
        {
            _payeRepository = payeRepository;
            _logger = logger;
        }

        public async Task<Unit> Handle(RemoveAccountPayeCommand request,CancellationToken cancellationToken)
        {
            try
            {
                await _payeRepository.RemovePayeScheme(request.AccountId, request.PayeRef);

                _logger.Info($"Paye scheme removed - account id: {request.AccountId}; paye ref: {request.PayeRef}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Could not remove account paye scheme");
                throw;
            }

            return Unit.Value;
        }
    }
}
