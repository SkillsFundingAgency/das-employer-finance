using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Messaging.AzureServiceBus.StructureMap;
using SFA.DAS.Messaging.AzureServiceBus;
using SFA.DAS.NLog.Logger;
using StructureMap;
using SFA.DAS.EmployerFinance.Configuration;

namespace SFA.DAS.EmployerFinance.DependencyResolution
{
    public class MessagePublisherRegistry : Registry
    {
        public MessagePublisherRegistry()
        {
            Policies.Add(new TopicMessagePublisherPolicy<EmployerFinanceConfiguration>("SFA.DAS.EmployerFinance", "1.0", new NLogLogger(typeof(TopicMessagePublisher))));
        }
    }
}
