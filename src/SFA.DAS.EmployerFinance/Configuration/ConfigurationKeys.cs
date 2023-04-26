namespace SFA.DAS.EmployerFinance.Configuration;

public static class ConfigurationKeys
{
    public const string EmployerFinance = "SFA.DAS.EmployerFinance.Web";
    public static string PaymentEventsApiClient => $"PaymentsEventsApi";
    public static string Hmrc => $"Hmrc";
    public static string TokenServiceApi => $"TokenServiceApi";

    public const string EmployerFinanceJobs = "SFA.DAS.EmployerFinance.Jobs";

    public const string AzureActiveDirectoryApiConfiguration = "AzureADApiAuthentication";
    public const string EncodingConfig = "SFA.DAS.Encoding";
}