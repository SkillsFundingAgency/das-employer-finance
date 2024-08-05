using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

public class RefreshPaymentDataResponse
{
    public ICollection<PaymentDetails> PaymentDetails { get; init; } = new List<PaymentDetails>();
}