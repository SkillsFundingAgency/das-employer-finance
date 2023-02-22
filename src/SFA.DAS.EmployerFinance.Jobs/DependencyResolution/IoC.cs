using SFA.DAS.EmployerFinance.DependencyResolution;

namespace SFA.DAS.EmployerFinance.Jobs.DependencyResolution;

public static class IoC
{
    public static void Initialize(Registry registry)
    {
        registry.IncludeRegistry<ConfigurationRegistry>();
        registry.IncludeRegistry<EmployerFinanceOuterApiRegistry>();
        registry.IncludeRegistry<DataRegistry>();
        registry.IncludeRegistry<DateTimeRegistry>();
        registry.IncludeRegistry<DefaultRegistry>();
    }
}