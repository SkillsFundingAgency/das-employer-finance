using Microsoft.Azure.ServiceBus;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class TopicClientFactory : ITopicClientFactory
{
    public ITopicClient CreateClient(string connectionString, string messageGroupName)
    {
        return new TopicClient(connectionString, messageGroupName);
    }
}