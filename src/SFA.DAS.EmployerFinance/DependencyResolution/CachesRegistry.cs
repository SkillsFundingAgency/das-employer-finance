﻿using SFA.DAS.Caches;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.EmployerFinance.Configuration;
using StructureMap;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class CachesRegistry :Registry
    {
        public CachesRegistry()
        {
            For<IInProcessCache>().Use<InProcessCache>().Singleton();

            For<IDistributedCache>().Use(c => c.GetInstance<IEnvironmentService>().IsCurrent(DasEnv.LOCAL)
                ? new LocalDevCache() as IDistributedCache
                : new RedisCache(c.GetInstance<EmployerFinanceConfiguration>().RedisConnectionString) as
                    IDistributedCache).Singleton();
        }
    }
}
