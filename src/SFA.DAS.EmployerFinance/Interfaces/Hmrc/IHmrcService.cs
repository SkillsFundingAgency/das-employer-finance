using HMRC.ESFA.Levy.Api.Types;

namespace SFA.DAS.EmployerFinance.Interfaces.Hmrc;

public interface IHmrcService
{
    Task<EmpRefLevyInformation> GetEmprefInformation(string empRef);
    Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef);
    Task<EnglishFractionDeclarations> GetEnglishFractions(string empRef, DateTime? fromDate);
    Task<DateTime> GetLastEnglishFractionUpdate();
    Task<LevyDeclarations> GetLevyDeclarations(string empRef, DateTime? fromDate);
}