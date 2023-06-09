using Microsoft.Azure.ServiceBus;

namespace SFA.DAS.EmployerFinance.Interfaces;

public interface ITopicClientFactory
{
    ITopicClient CreateClient(string connectionString, string messageGroupName);
}