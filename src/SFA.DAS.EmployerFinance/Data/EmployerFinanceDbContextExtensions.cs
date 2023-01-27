using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Data
{
    public static class EmployerFinanceDbContextExtensions
    {
        public static async Task<IEnumerable<AccountTransfer>> GetTransfersByTargetAccountId(this EmployerFinanceDbContext db, long accountId, long targetAccountId, string periodEnd)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId);
            parameters.Add("@targetAccountId", targetAccountId);
            parameters.Add("@periodEnd", periodEnd);

            var result = await db.Database.GetDbConnection().QueryAsync<AccountTransfer>(
                sql: "[employer_financial].[GetTransferTransactionDetails]",
                param: parameters,
                commandType: System.Data.CommandType.StoredProcedure
                );

            return result.ToList();
        }
    }
}

