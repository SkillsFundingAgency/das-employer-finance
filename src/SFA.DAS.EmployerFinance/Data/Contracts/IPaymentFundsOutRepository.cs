using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IPaymentFundsOutRepository
{
    Task<IEnumerable<PaymentFundsOut>> GetPaymentFundsOut(long accountId);
}