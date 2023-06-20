using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IEnglishFractionRepository
{
    Task CreateEmployerFraction(DasEnglishFraction fractions, string employerReference);
    Task<IEnumerable<DasEnglishFraction>> GetAllEmployerFractions(string employerReference);
    Task<DateTime> GetLastUpdateDate();
    Task SetLastUpdateDate(DateTime dateUpdated);
}