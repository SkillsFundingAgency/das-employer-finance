using System.Text.RegularExpressions;
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
        (t.Name != null && Regex.IsMatch(t.Name, @"Event(V\d+)?$"))
        || (typeof(IEvent).IsAssignableFrom(t) && t != typeof(IEvent))
        || IsSfaMessage(t, "Messages.Events");

    public static bool IsCommand(Type t) =>
        (t.Name != null && Regex.IsMatch(t.Name, @"Command(V\d+)?$"))
        || (typeof(ICommand).IsAssignableFrom(t) && t != typeof(ICommand))
        || IsSfaMessage(t, "Messages.Commands");

    public static bool IsSfaMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           t.Namespace.StartsWith("SFA.DAS") &&
           t.Namespace.EndsWith(namespaceSuffix);

}