﻿using System.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;

namespace SFA.DAS.EmployerFinance.Jobs.DependencyResolution;

public class DefaultRegistry : Registry
{
    private const string AzureResource = "https://database.windows.net/";

    public DefaultRegistry()
    {
        Scan(s =>
        {
            s.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith("SFA.DAS"));
            s.RegisterConcreteTypesAgainstTheFirstInterface();
        });
    }
}