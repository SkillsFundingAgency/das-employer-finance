using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface ILevyFundsInRepository
{
    Task<IEnumerable<LevyFundsIn>> GetLevyFundsIn(long accountId);
}