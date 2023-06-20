using NServiceBus;
using SFA.DAS.EmployerFinance.Messages.Commands;

namespace SFA.DAS.EmployerFinance.Extensions;

public static class RoutingExtensions
{
    public static void AddRouting(this RoutingSettings routing)
    {
        routing.RouteToEndpoint(
            typeof(ImportLevyDeclarationsCommand).Assembly,
            typeof(ImportLevyDeclarationsCommand).Namespace,
            "SFA.DAS.EmployerFinance.MessageHandlers"
        );

    }
}