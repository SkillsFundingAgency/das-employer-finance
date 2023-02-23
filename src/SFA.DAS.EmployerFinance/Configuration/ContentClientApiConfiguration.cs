﻿using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Configuration;

public class ContentClientApiConfiguration : IContentClientApiConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string Tenant { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string IdentifierUri { get; set; }
}