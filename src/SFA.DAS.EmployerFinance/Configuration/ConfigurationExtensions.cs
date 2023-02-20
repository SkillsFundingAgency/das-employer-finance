﻿using System;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.EmployerFinance.Configuration;

public static class ConfigurationExtensions
{
    public static bool IsDev(this IConfiguration configuration)
    {
        return configuration["EnvironmentName"].Equals("Development", StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsLocal(this IConfiguration configuration)
    {
        return configuration["EnvironmentName"].StartsWith("LOCAL", StringComparison.CurrentCultureIgnoreCase);
    }
    public static bool IsTest(this IConfiguration configuration)
    {
        return configuration["EnvironmentName"].StartsWith("Test", StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsDevOrLocal(this IConfiguration configuration)
    {
        return IsDev(configuration) || IsLocal(configuration);
    }
}