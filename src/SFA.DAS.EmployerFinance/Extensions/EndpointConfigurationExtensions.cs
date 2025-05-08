using NServiceBus;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;

namespace SFA.DAS.EmployerFinance.Extensions;

public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder, bool isLocal)
    {
        if (isLocal)
        {
            var transport = config.UseTransport<LearningTransport>();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            transport.Routing().AddRouting();
        }
        else
        {
            config.UseAzureServiceBusTransport(connectionStringBuilder(), s => s.AddRouting());
        }

        config.UseNewMessageConventions();

        return config;
    }

    public static EndpointConfiguration UseNewMessageConventions(this EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.Conventions()
            .DefiningMessagesAs(IsMessage)
            .DefiningEventsAs(IsEvent)
            .DefiningCommandsAs(IsCommand);

        return endpointConfiguration;
    }

    public static bool IsMessage(Type t) => IsSfaMessage(t, "Messages");

    public static bool IsEvent(Type t) =>
        (t.FullName != null && t.FullName.EndsWith("Event")) || IsSfaMessage(t, "Messages.Events");

    public static bool IsCommand(Type t) => (t.FullName != null && t.FullName.EndsWith("Command")) ||
                                            IsSfaMessage(t, "Messages.Commands");

    public static bool IsSfaMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           t.Namespace.StartsWith("SFA.DAS") &&
           t.Namespace.EndsWith(namespaceSuffix);

}