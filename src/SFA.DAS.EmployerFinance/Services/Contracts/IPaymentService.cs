using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IPaymentService
{
    Task<ICollection<PaymentDetails>> GetAccountPayments(string periodEnd, long employerAccountId, Guid correlationId);
    Task<PaymentDetails> AddSinglePaymentDetailsMetadata(long employerAccountId, PaymentDetails payment);
    Task<IEnumerable<AccountTransfer>> GetAccountTransfers(string periodEnd, long receiverAccountId, Guid correlationId);
}