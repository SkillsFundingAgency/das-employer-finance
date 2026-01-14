using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;

public class UpdatePaymentMetadataCommand : IRequest
{
    public PaymentMetaData PaymentMetadata { get; set; }
}
