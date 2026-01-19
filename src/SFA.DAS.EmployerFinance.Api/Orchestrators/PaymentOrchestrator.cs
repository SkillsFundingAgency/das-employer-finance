using AutoMapper;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentOrchestrator
{
    private readonly IMediator _mediator;

    public PaymentOrchestrator(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<PaymentMetaDataResponse> UpdatePaymentMetaDataStaging(Guid paymentId, PaymentMetaDataStaging paymentMetadata)
    {
        return await _mediator.Send(new UpdatePaymentMetadataStagingCommand
        {
            PaymentMetadataStaging = paymentMetadata,
            PaymentId = paymentId
        });
    }
}
