using AutoMapper;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentOrchestrator> _logger;
    private readonly IMapper _mapper;

    public PaymentOrchestrator(
        IMediator mediator,
        ILogger<PaymentOrchestrator> logger,
        IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<bool> UpdatePaymentMetadata(Guid paymentId, PaymentMetaData paymentMetadata)
    {
        await _mediator.Send(new UpdatePaymentMetadataCommand
        {
            PaymentMetadata = paymentMetadata,
            PaymentId = paymentId
        });

        return true;
    }
}
