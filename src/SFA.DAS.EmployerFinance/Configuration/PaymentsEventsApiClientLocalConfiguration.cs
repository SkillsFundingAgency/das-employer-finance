using SFA.DAS.Provider.Events.Api.Client.Configuration;

namespace SFA.DAS.EmployerFinance.Configuration;

public class PaymentsEventsApiClientLocalConfiguration : PaymentsEventsApiClientConfiguration
{
    public bool PaymentsDisabled { get; set; }
}