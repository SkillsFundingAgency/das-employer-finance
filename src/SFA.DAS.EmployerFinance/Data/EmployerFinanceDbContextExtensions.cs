using System.Linq;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Data;

public static class EmployerFinanceDbContextExtensions
{
    public static async Task<IEnumerable<AccountTransfer>> GetTransfersByTargetAccountId(this EmployerFinanceDbContext db, long accountId, long targetAccountId, string periodEnd)
    {
<<<<<<< HEAD
        public static async Task<IEnumerable<AccountTransfer>> GetTransfersByTargetAccountId(this Lazy<EmployerFinanceDbContext> db, long accountId, long targetAccountId, string periodEnd)
        {
            var parameters = new DynamicParameters();
=======
        var parameters = new DynamicParameters();
>>>>>>> 39069e7626affab60ab1e0fb22c8537867d2005b

        parameters.Add("@accountId", accountId);
        parameters.Add("@targetAccountId", targetAccountId);
        parameters.Add("@periodEnd", periodEnd);

<<<<<<< HEAD
            var result = await db.Value.Database.GetDbConnection().QueryAsync<AccountTransfer>(
                sql: "[employer_financial].[GetTransferTransactionDetails]",
                param: parameters,
                commandType: System.Data.CommandType.StoredProcedure
                );
=======
        var result = await db.Database.GetDbConnection().QueryAsync<AccountTransfer>(
            sql: "[employer_financial].[GetTransferTransactionDetails]",
            param: parameters,
            commandType: CommandType.StoredProcedure
        );
>>>>>>> 39069e7626affab60ab1e0fb22c8537867d2005b

        return result.ToList();
    }
}