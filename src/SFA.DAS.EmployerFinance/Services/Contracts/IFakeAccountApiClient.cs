using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public class FakeAccountApiClient(IAccountApiClient innerClient) : IAccountApiClient
{
    public new Task<AccountDetailViewModel> GetAccount(long accountId)
    {
        return Task.FromResult(new AccountDetailViewModel
        {
            AccountId = accountId,
            DasAccountName = $"Test Employer {accountId}",
            ApprenticeshipEmployerType = nameof(ApprenticeshipEmployerType.Levy)
        });
    }

    public new Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
    {
        return Task.FromResult(new AccountDetailViewModel
        {
            HashedAccountId = hashedAccountId,
            DasAccountName = $"Test Employer {hashedAccountId}",
            ApprenticeshipEmployerType = nameof(ApprenticeshipEmployerType.Levy)
        });
    }

    // --- Everything else just delegates to the real client ---
    public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId) => innerClient.GetAccountUsers(accountId);
    public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId) => innerClient.GetAccountUsers(accountId);
    public Task<EmployerAgreementView> GetEmployerAgreement(string accountId, string legalEntityId, string agreementId) => innerClient.GetEmployerAgreement(accountId, legalEntityId, agreementId);
    public Task<ICollection<ResourceViewModel>> GetLegalEntitiesConnectedToAccount(string accountId) => innerClient.GetLegalEntitiesConnectedToAccount(accountId);
    public Task<LegalEntityViewModel> GetLegalEntity(string accountId, long id) => innerClient.GetLegalEntity(accountId, id);
    public Task<ICollection<LevyDeclarationViewModel>> GetLevyDeclarations(string accountId) => innerClient.GetLevyDeclarations(accountId);
    public Task<PagedApiResponseViewModel<AccountLegalEntityViewModel>> GetPageOfAccountLegalEntities(int pageNumber = 1, int pageSize = 1000) => innerClient.GetPageOfAccountLegalEntities(pageNumber, pageSize);
    public Task<PagedApiResponseViewModel<AccountWithBalanceViewModel>> GetPageOfAccounts(int pageNumber = 1, int pageSize = 1000, DateTime? toDate = null) => innerClient.GetPageOfAccounts(pageNumber, pageSize, toDate);
    public Task<ICollection<ResourceViewModel>> GetPayeSchemesConnectedToAccount(string accountId) => innerClient.GetPayeSchemesConnectedToAccount(accountId);
    public Task<T> GetResource<T>(string uri) => innerClient.GetResource<T>(uri);
    public Task<StatisticsViewModel> GetStatistics() => innerClient.GetStatistics();
    public Task<TransactionsViewModel> GetTransactions(string accountId, int year, int month) => innerClient.GetTransactions(accountId, year, month);
    public Task<ICollection<TransactionSummaryViewModel>> GetTransactionSummary(string accountId) => innerClient.GetTransactionSummary(accountId);
    public Task<ICollection<TransferConnectionViewModel>> GetTransferConnections(string accountHashedId) => innerClient.GetTransferConnections(accountHashedId);
    public Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId) => innerClient.GetUserAccounts(userId);
    public Task Ping() => innerClient.Ping();
    public Task<ICollection<LegalEntityViewModel>> GetLegalEntityDetailsConnectedToAccount(string accountId) => innerClient.GetLegalEntityDetailsConnectedToAccount(accountId);
}