using System;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.EmployerFinance.Web.Extensions;

public static class ConfigurationExtensions
{
    public static bool UseGovUkSignIn(this IConfiguration configuration)
    {
        return configuration["EmployerFinanceConfiguration:UseGovSignIn"] != null &&
               configuration["EmployerFinanceConfiguration:UseGovSignIn"]
                   .Equals("true", StringComparison.CurrentCultureIgnoreCase);
    }

}