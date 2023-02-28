namespace SFA.DAS.EmployerFinance.Configuration;

public static class ConfigurationKeys
{
    public const string EmployerFinance = "SFA.DAS.EmployerFinance";
    public static string PaymentEventsApiClient => $"{EmployerFinance}:PaymentsEventsApi";
    public static string CommitmentsV2ApiClient => $"{EmployerFinance}:CommitmentsApi";
    public const string Features = "SFA.DAS.EmployerFinance.Features";
    public const string NotificationsApiClient = "SFA.DAS.EmployerFinance.Notifications";
    public const string AzureActiveDirectoryApiConfiguration = "AzureADApiAuthentication";
    public const string EncodingConfig = "SFA.DAS.Encoding";
}