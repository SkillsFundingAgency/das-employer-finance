using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;

public class UpdatePaymentMetadataStagingCommand : IRequest<PaymentMetaDataResponse>
{
    public PaymentMetaDataStaging PaymentMetadataStaging { get; set; }

    public Guid PaymentId { get; set; }
}
