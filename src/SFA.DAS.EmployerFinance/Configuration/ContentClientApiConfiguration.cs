using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Configuration;

public class ContentClientApiConfiguration : IContentClientApiConfiguration
{
    public static string SectionName => "ContentApi";
    public string ApiBaseUrl { get; set; }
    public string IdentifierUri { get; set; }
}