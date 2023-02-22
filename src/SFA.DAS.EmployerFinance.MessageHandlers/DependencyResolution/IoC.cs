using SFA.DAS.EmployerFinance.DependencyResolution;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.StructureMap;

namespace SFA.DAS.EmployerFinance.MessageHandlers.DependencyResolution;

public static class IoC
{
    public static void Initialize(Registry registry)
    {
        registry.IncludeRegistry<CachesRegistry>();
        registry.IncludeRegistry<ConfigurationRegistry>();
        //registry.IncludeRegistry<DataRegistry>();
        registry.IncludeRegistry<DefaultRegistry>();
        registry.IncludeRegistry<EventsRegistry>();
        //registry.IncludeRegistry<LoggerRegistry>();
        //registry.IncludeRegistry<MapperRegistry>();
        registry.IncludeRegistry<MediatorRegistry>();
        registry.IncludeRegistry<MessagePublisherRegistry>();
        registry.IncludeRegistry<NotificationsRegistry>();
        registry.IncludeRegistry<NServiceBusUnitOfWorkRegistry>();
        //c.AddRegistry<RepositoriesRegistry>();
        //c.AddRegistry<ReadStoreDataRegistry>();
        //c.AddRegistry<ReadStoreMediatorRegistry>();
        //c.AddRegistry<EntityFrameworkUnitOfWorkRegistry<EmployerAccountsDbContext>>();
        //c.AddRegistry<ExecutionPoliciesRegistry>();
    }
}