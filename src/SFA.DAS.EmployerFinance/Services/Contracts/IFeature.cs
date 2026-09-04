namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IFeature
{
    bool IsFeatureEnabled(string feature);
}