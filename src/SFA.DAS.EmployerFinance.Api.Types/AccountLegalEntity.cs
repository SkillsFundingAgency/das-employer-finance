namespace SFA.DAS.EmployerFinance.Api.Types;

public class AccountLegalEntity
{
    public long Id { get; set; }
    public Account Account { get; set; }
    public long AccountId { get; set; }
    public long? PendingAgreementId { get; set; }
    public long? SignedAgreementId { get; set; }

    public int? SignedAgreementVersion { get; set; }
}
