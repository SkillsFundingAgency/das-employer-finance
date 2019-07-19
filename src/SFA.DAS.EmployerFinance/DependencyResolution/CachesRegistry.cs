﻿using SFA.DAS.Caches;
using SFA.DAS.Configuration;
using SFA.DAS.EmployerFinance.Configuration;
using StructureMap;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class CachesRegistry : Registry
    {
        public CachesRegistry()
        {
            For<IInProcessCache>().Use<InProcessCache>().Singleton();

            if (ConfigurationHelper.IsEnvironmentAnyOf(Environment.Local))
            {
                For<IDistributedCache>().Use<LocalDevCache>().Singleton();
            }
            else
            {
                For<IDistributedCache>().Use(c=> 
                    new RedisCache(c.GetInstance<EmployerFinanceConfiguration>().RedisConnectionString))
                    .Singleton();
            }
        }
    }
}