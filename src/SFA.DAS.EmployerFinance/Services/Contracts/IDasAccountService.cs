namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IDasAccountService
{
    Task UpdatePayeScheme(string empRef);
}