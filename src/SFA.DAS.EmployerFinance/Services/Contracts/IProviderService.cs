namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IProviderService
{
    Task<Models.ApprenticeshipProvider.Provider> Get(long ukPrn);
}