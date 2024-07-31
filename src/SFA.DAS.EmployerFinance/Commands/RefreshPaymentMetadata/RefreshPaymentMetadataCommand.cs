
namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;

public class RefreshPaymentMetadataCommand : IRequest
{
    public long AccountId { get; init; }
    public Guid PaymentId { get; set; }
}