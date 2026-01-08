using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Extensions;

[ExcludeFromCodeCoverage]
public static class DatabaseExtensions
{

    public static DbConnection GetSqlConnection(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        return new SqlConnection(connectionString);
    }
}