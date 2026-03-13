using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentMetaDataOrchestrator
{
    private readonly IMediator _mediator;

    public PaymentMetaDataOrchestrator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<PaymentMetaDataResponse> UpdatePaymentMetaDataStaging(
        Guid paymentId,
        PaymentMetaDataStaging paymentMetadata)
    {
        return await _mediator.Send(new UpdatePaymentMetadataStagingCommand
        {
            PaymentId = paymentId,
            PaymentMetadataStaging = paymentMetadata
        });
    }
}