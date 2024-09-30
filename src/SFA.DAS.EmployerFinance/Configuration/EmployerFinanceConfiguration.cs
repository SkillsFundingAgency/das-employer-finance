namespace SFA.DAS.EmployerFinance.Configuration;

public class EmployerFinanceConfiguration
{
    public string DatabaseConnectionString { get; set; }
    public int DefaultCacheExpirationInMinutes { get; set; }
    public decimal TransferAllowancePercentage { get; set; }
    public string NServiceBusLicense { get; set; }
    public string ServiceBusConnectionString { get; set; }
    public string RedisConnectionString { get; set; }
    public virtual int FundsExpiryPeriod { get; set; }
    public string EmployerFinanceBaseUrl { get; set; }
}

public class EmployerFinanceJobsConfiguration : EmployerFinanceConfiguration
{
    public string LegacyServiceBusConnectionString { get; set; }
    public string MessageServiceBusConnectionString => LegacyServiceBusConnectionString;
}

public class EmployerFinanceWebConfiguration : EmployerFinanceConfiguration
{
    public string ApplicationId { get; set; }
    public string ReservationsBaseUrl { get; set; }
    public string DataProtectionKeysDatabase { get; set; }
    public string EmployerAccountsBaseUrl { get; set; }
    public string EmployerCommitmentsV2BaseUrl { get; set; }
    public string EmployerPortalBaseUrl { get; set; }
    public string EmployerProjectionsBaseUrl { get; set; }
    public string EmployerRecruitBaseUrl { get; set; }
    public string LevyTransferMatchingBaseUrl { get; set; }
}

public class ZenDeskConfiguration
{
    public string ZenDeskHelpCentreUrl { get; set; }
    public string ZenDeskSnippetKey { get; set; }
    public string ZenDeskSectionId { get; set; }
    public string ZenDeskCobrowsingSnippetKey { get; set; }
}