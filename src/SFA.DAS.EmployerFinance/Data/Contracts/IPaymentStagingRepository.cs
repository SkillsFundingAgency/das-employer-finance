using SFA.DAS.EmployerFinance.Models.PaymentStaging;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IPaymentStagingRepository
{
    Task<BulkPaymentsIngestResult> BulkInsertPaymentsAsync(List<PaymentStagingModel> payments);
}