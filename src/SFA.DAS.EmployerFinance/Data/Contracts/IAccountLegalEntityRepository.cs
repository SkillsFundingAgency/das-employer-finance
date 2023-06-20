namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IAccountLegalEntityRepository
{
    Task CreateAccountLegalEntity(long id, long? pendingAgreementId, long? signedAgreementId,
        int? signedAgreementVersion, long accountId, long legalEntityId);

    Task SignAgreement(long signedAgreementId, int signedAgreementVersion, long accountId, long legalEntityId);

    Task RemoveAccountLegalEntity(long id);
}