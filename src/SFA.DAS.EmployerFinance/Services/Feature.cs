using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class Feature(IConfiguration configuration) : IFeature
{
    public bool IsFeatureEnabled(string feature)
    {
        string featureValue = configuration[$"Features:{feature}"];
        return !string.IsNullOrWhiteSpace(featureValue) && bool.Parse(featureValue);
    }
}