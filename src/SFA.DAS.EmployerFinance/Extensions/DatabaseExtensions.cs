using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace SFA.DAS.EmployerFinance.Extensions;

public static class DatabaseExtensions
{
    private const string AzureResource = "https://database.windows.net/";

    public static DbConnection GetSqlConnection(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
        bool useManagedIdentity = !connectionStringBuilder.IntegratedSecurity && string.IsNullOrEmpty(connectionStringBuilder.UserID);

        if (!useManagedIdentity)
        {
            return new SqlConnection(connectionString);
        }

        var azureServiceTokenProvider = CreateChainedTokenCredential();

        return new SqlConnection
        {
            ConnectionString = connectionString,
            AccessToken = azureServiceTokenProvider.GetToken(new TokenRequestContext(scopes: [AzureResource])).Token
        };
    }

    private static ChainedTokenCredential CreateChainedTokenCredential()
    {
        #if DEBUG
                return new ChainedTokenCredential(
                    new AzureCliCredential(),
                    new VisualStudioCodeCredential(),
                    new VisualStudioCredential());
        #else
                return new ChainedTokenCredential(
                   new ManagedIdentityCredential(),
                   new AzureCliCredential(),
                   new VisualStudioCodeCredential(),
                   new VisualStudioCredential());
        #endif
    }
}