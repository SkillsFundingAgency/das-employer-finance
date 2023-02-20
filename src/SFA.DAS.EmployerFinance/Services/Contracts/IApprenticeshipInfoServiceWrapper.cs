using SFA.DAS.EmployerFinance.Models.ApprenticeshipCourse;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IApprenticeshipInfoServiceWrapper
{
    Task<StandardsView> GetStandardsAsync(bool refreshCache = false);
    Task<FrameworksView> GetFrameworksAsync(bool refreshCache = false);
}