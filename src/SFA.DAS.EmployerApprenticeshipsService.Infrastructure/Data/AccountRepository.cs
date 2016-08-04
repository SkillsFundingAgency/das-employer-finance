﻿using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;

namespace SFA.DAS.EmployerApprenticeshipsService.Infrastructure.Data
{
    public class AccountRepository : BaseRepository, IAccountRepository
    {
        public AccountRepository(EmployerApprenticeshipsServiceConfiguration configuration, ILogger logger)
            : base(configuration, logger)
        {
        }

        public async Task<long> CreateAccount(string userRef, string employerNumber, string employerName, string employerRef)
        {
            return await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@userRef", new Guid(userRef), DbType.Guid);
                parameters.Add("@employerNumber", employerNumber, DbType.String);
                parameters.Add("@employerName", employerName, DbType.String);
                parameters.Add("@employerRef", employerRef, DbType.String);
                parameters.Add("@accountId", null, DbType.Int64, ParameterDirection.Output, 8);

                await c.ExecuteAsync(
                    sql: "[dbo].[CreateAccount]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                return parameters.Get<long>("@accountId");
            });
        }
    }
}