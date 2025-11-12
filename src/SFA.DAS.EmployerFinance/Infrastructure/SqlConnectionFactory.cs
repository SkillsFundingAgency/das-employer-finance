using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using SFA.DAS.Api.Common.Interfaces;

namespace SFA.DAS.EmployerFinance.Infrastructure;

public interface ISqlConnectionFactory
{
    DbConnection Create(string connectionString);
}

public class ManagedIdentitySqlConnectionFactory(IAzureClientCredentialHelper credentialHelper) : ISqlConnectionFactory
{
    private const string AzureSqlScope = "https://database.windows.net/";
    private readonly IAzureClientCredentialHelper _credentialHelper = credentialHelper ?? throw new ArgumentNullException(nameof(credentialHelper));

    public DbConnection Create(string connectionString)
    {
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        var builder = new SqlConnectionStringBuilder(connectionString);
        var useManagedIdentity = !builder.IntegratedSecurity && string.IsNullOrEmpty(builder.UserID);

        if (!useManagedIdentity)
        {
            return new SqlConnection(connectionString);
        }

        var accessToken = _credentialHelper.GetAccessTokenAsync(AzureSqlScope).Result;

        var connection = new SqlConnection(connectionString)
        {
            AccessToken = accessToken
        };

        return connection;
    }
}

