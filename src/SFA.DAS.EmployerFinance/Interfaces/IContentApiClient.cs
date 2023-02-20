namespace SFA.DAS.EmployerFinance.Interfaces;

public interface IContentApiClient
{
    Task<string> Get(string type, string applicationId);
}