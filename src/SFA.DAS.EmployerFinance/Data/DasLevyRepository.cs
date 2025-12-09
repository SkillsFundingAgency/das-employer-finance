using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;
using SFA.DAS.EmployerFinance.Queries.GetAccounts;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Data;

[ExcludeFromCodeCoverage]
public class DasLevyRepository : IDasLevyRepository
{
    private readonly EmployerFinanceConfiguration _configuration;
    private readonly Lazy<EmployerFinanceDbContext> _db;
    private readonly ICurrentDateTime _currentDateTime;

    public DasLevyRepository(EmployerFinanceConfiguration configuration, Lazy<EmployerFinanceDbContext> db, ICurrentDateTime currentDateTime)
    {
        _configuration = configuration;
        _db = db;
        _currentDateTime = currentDateTime;
    }

    public async Task<PaymentDetails> GetPaymentForPaymentDetails(Guid paymentId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@paymentId", paymentId, DbType.Guid);

        return await _db.Value.Database
            .GetDbConnection()
            .QuerySingleOrDefaultAsync<PaymentDetails>(
                sql: "[employer_financial].[GetPaymentForPaymentDetails]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<PaymentDetails>> GetPaymentsWithMissingMetadata()
    {
        return await _db.Value.Database
            .GetDbConnection()
            .QueryAsync<PaymentDetails>(
                sql: "[employer_financial].[GetPaymentsWithMissingMetadata]",
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
    }

    public async Task CreateEmployerDeclarations(IEnumerable<DasDeclaration> declarations, string empRef, long accountId)
    {
        foreach (var dasDeclaration in declarations)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@LevyDueYtd", dasDeclaration.LevyDueYtd, DbType.Decimal);
            parameters.Add("@LevyAllowanceForYear", dasDeclaration.LevyAllowanceForFullYear, DbType.Decimal);
            parameters.Add("@AccountId", accountId, DbType.Int64);
            parameters.Add("@EmpRef", empRef, DbType.String);
            parameters.Add("@PayrollYear", dasDeclaration.PayrollYear, DbType.String);
            parameters.Add("@PayrollMonth", dasDeclaration.PayrollMonth, DbType.Int16);
            parameters.Add("@SubmissionDate", dasDeclaration.SubmissionDate, DbType.DateTime);
            parameters.Add("@SubmissionId", dasDeclaration.Id, DbType.Int64);
            parameters.Add("@HmrcSubmissionId", dasDeclaration.SubmissionId, DbType.Int64);
            parameters.Add("@CreatedDate", DateTime.UtcNow, DbType.DateTime);

            if (dasDeclaration.DateCeased.HasValue && dasDeclaration.DateCeased != DateTime.MinValue)
            {
                parameters.Add("@DateCeased", dasDeclaration.DateCeased, DbType.DateTime);
            }

            if (dasDeclaration.InactiveFrom.HasValue && dasDeclaration.InactiveFrom != DateTime.MinValue)
            {
                parameters.Add("@InactiveFrom", dasDeclaration.InactiveFrom, DbType.DateTime);
            }

            if (dasDeclaration.InactiveTo.HasValue && dasDeclaration.InactiveTo != DateTime.MinValue)
            {
                parameters.Add("@InactiveTo", dasDeclaration.InactiveTo, DbType.DateTime);
            }

            parameters.Add("@EndOfYearAdjustment", dasDeclaration.EndOfYearAdjustment, DbType.Boolean);
            parameters.Add("@EndOfYearAdjustmentAmount", dasDeclaration.EndOfYearAdjustmentAmount, DbType.Decimal);
            parameters.Add("@NoPaymentForPeriod", dasDeclaration.NoPaymentForPeriod, DbType.Boolean);

            await _db.Value.Database.GetDbConnection().ExecuteAsync(
                sql: "[employer_financial].[CreateDeclaration]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
        }
    }

    public Task CreateNewPeriodEnd(PeriodEnd periodEnd)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@PeriodEndId", periodEnd.PeriodEndId, DbType.String);
        parameters.Add("@CalendarPeriodMonth", periodEnd.CalendarPeriodMonth, DbType.Int32);
        parameters.Add("@CalendarPeriodYear", periodEnd.CalendarPeriodYear, DbType.Int32);
        parameters.Add("@AccountDataValidAt", periodEnd.AccountDataValidAt, DbType.DateTime);
        parameters.Add("@CommitmentDataValidAt", periodEnd.CommitmentDataValidAt, DbType.DateTime);
        parameters.Add("@CompletionDateTime", periodEnd.CompletionDateTime, DbType.DateTime);
        parameters.Add("@PaymentsForPeriod", periodEnd.PaymentsForPeriod, DbType.String);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[CreatePeriodEnd]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task CreatePayments(IEnumerable<PaymentDetails> payments)
    {
        // This could be a candidate for refactoring to use SqlBulkCopy
        // https://timdeschryver.dev/blog/faster-sql-bulk-inserts-with-csharp#table-valued-parameter

        var batches = payments.Batch(1000).Select(b => b.ToPaymentsDataTable());

        foreach (var batch in batches)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@payments", batch.AsTableValuedParameter("[employer_financial].[PaymentsTable]"));

            await _db.Value.Database.GetDbConnection().ExecuteAsync(
                sql: "[employer_financial].[CreatePayments]",
                param: parameters,
                commandTimeout: 300,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
        }
    }

    public async Task UpdatePaymentMetadata(PaymentDetails details)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@ProviderName", details.ProviderName, DbType.String);
        parameters.Add("@ApprenticeName", details.ApprenticeName, DbType.String);
        parameters.Add("@IsHistoricProviderName", details.IsHistoricProviderName, DbType.Boolean);
        parameters.Add("@CourseName", details.CourseName, DbType.String);
        parameters.Add("@CourseLevel", details.CourseLevel, DbType.Int32);
        parameters.Add("@CourseStartDate", details.CourseStartDate, DbType.DateTime);
        parameters.Add("@PathwayName", details.PathwayName, DbType.String);
        parameters.Add("@PaymentMetaDataId", details.PaymentMetaDataId, DbType.Int64);

        await _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[UpdatePaymentMetadata]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<ISet<Guid>> GetAccountPaymentIds(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<Guid>(
            sql: "[employer_financial].[GetAccountPaymentIds]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return new HashSet<Guid>(result);
    }

    public async Task<GetAccountPaymentIdsResponse> GetAccountPaymentIdsLinq(long accountId, int pageSize, int pageNumber)
    {
        var totalCount = await _db.Value.Payments
         .Where(p => p.EmployerAccountId == accountId)
         .CountAsync();

        var paymentIds = await _db.Value.Payments
            .Where(p => p.EmployerAccountId == accountId)
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.Id)
            .ToListAsync();

        return new GetAccountPaymentIdsResponse
        {
            PaymentIds = paymentIds,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public Task<IEnumerable<long>> GetEmployerDeclarationSubmissionIds(string empRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@empRef", empRef, DbType.String);

        return _db.Value.Database.GetDbConnection().QueryAsync<long>(
            sql: "[employer_financial].[GetLevyDeclarationSubmissionIdsByEmpRef]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<DasDeclaration> GetLastSubmissionForScheme(string empRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@empRef", empRef, DbType.String);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<DasDeclaration>(
            sql: "[employer_financial].[GetLastLevyDeclarations_ByEmpRef]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.SingleOrDefault();
    }

    public Task<IEnumerable<PeriodEnd>> GetAllPeriodEnds()
    {
        return _db.Value.Database.GetDbConnection().QueryAsync<PeriodEnd>(
            sql: "[employer_financial].[GetAllPeriodEnds]",
            param: null,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PeriodEnd> GetPeriodEndById(string periodEndId)
    {
        return await _db.Value.PeriodEnds
        .SingleOrDefaultAsync(pe => pe.PeriodEndId == periodEndId);
    }

    public async Task<DasDeclaration> GetSubmissionByEmprefPayrollYearAndMonth(string empRef, string payrollYear, short payrollMonth)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@empRef", empRef, DbType.String);
        parameters.Add("@payrollYear", payrollYear, DbType.String);
        parameters.Add("@payrollMonth", payrollMonth, DbType.Int32);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<DasDeclaration>(
            sql: "[employer_financial].[GetLevyDeclaration_ByEmpRefPayrollMonthPayrollYear]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.SingleOrDefault();
    }

    public async Task<DasDeclaration> GetEffectivePeriod12Declaration(string empRef, string payrollYear, DateTime yearEndAdjustmentCutOff)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@empRef", empRef, DbType.String);
        parameters.Add("@payrollYear", payrollYear, DbType.String);
        parameters.Add("@yearEndAdjustmentCutOff", yearEndAdjustmentCutOff, DbType.DateTime);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<DasDeclaration>(
            sql: "[employer_financial].[GetEffectivePeriod12Declaration]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.SingleOrDefault();
    }

    public Task<decimal> ProcessDeclarations(long accountId, string empRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@AccountId", accountId, DbType.Int64);
        parameters.Add("@EmpRef", empRef, DbType.String);
        parameters.Add("@currentDate", _currentDateTime.Now, DbType.DateTime);
        parameters.Add("@expiryPeriod", _configuration.FundsExpiryPeriod, DbType.Int32);

        return _db.Value.Database.GetDbConnection().QuerySingleAsync<decimal>(
            sql: "[employer_financial].[ProcessDeclarationsTransactions]",
            param: parameters,
            commandTimeout: 120,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public Task ProcessPaymentData(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);

        return _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_financial].[ProcessPaymentDataTransactions]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<string> FindHistoricalProviderName(long ukprn)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@ukprn", ukprn, DbType.Int64);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<string>(
            sql: "[employer_financial].[GetLastKnownProviderNameForUkprn]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.FirstOrDefault();
    }

    public async Task<List<LevyDeclarationItem>> GetAccountLevyDeclarations(long accountId)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<LevyDeclarationItem>(
            sql: "[employer_financial].[GetLevyDeclarations_ByAccountId]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.ToList();
    }
    
    public async Task<List<LevyDeclarationItem>> GetAccountLevyDeclarationsForPreviousMonths(long accountId, int months)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@months", months, DbType.Int32);

        const string sqlQuery = @"
                                SELECT
	                                EmpRef
	                                ,TotalAmount
	                                ,PayrollYear
	                                ,PayrollMonth
                                FROM [employer_financial].[GetLevyDeclarationAndTopUp]
                                WHERE EmpRef IN
                                (
	                                SELECT
		                                EmpRef
	                                FROM [employer_financial].LevyDeclaration
	                                WHERE AccountId = @AccountId
                                )
                                AND (LastSubmission = 1 OR EndOfYearAdjustment = 1)
                                AND AccountId = @AccountId
                                AND SubmissionDate >= DATEADD(month, -@months, GETDATE())
                                ORDER BY SubmissionDate ASC"; 

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<LevyDeclarationItem>(
            sql: sqlQuery,
            param: parameters,
            commandTimeout: 60,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);

        return result.ToList();
    }

    public async Task<List<LevyDeclarationItem>> GetAccountLevyDeclarations(long accountId, string payrollYear, short payrollMonth)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@payrollYear", payrollYear, DbType.String);
        parameters.Add("@payrollMonth", payrollMonth, DbType.Int16);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<LevyDeclarationItem>(
            sql: "[employer_financial].[GetLevyDeclarations_ByAccountPayrollMonthPayrollYear]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.ToList();
    }

    public async Task<List<AccountBalance>> GetAccountBalances(List<long> accountIds)
    {
        var accountParametersTable = new AccountIdUserTableParam(accountIds);

        accountParametersTable.Add("@allowancePercentage", _configuration.TransferAllowancePercentage, DbType.Decimal);

        var result = await _db.Value.Database.GetDbConnection().QueryAsync<AccountBalance>(
            sql: "[employer_financial].[GetAccountBalance_ByAccountIds]",
            param: accountParametersTable,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);

        return result.ToList();
    }

    public Task<IEnumerable<DasEnglishFraction>> GetEnglishFractionHistory(long accountId, string empRef)
    {
        var parameters = new DynamicParameters();

        parameters.Add("@accountId", accountId, DbType.Int64);
        parameters.Add("@empRef", empRef, DbType.String);

        return _db.Value.Database.GetDbConnection().QueryAsync<DasEnglishFraction>(
            sql: "[employer_financial].[GetEnglishFraction_ByEmpRef]",
            param: parameters,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
    }

    public async Task<List<DasEnglishFraction>> GetEnglishFractionCurrent(long accountId, string[] empRefs)
    {
        var currentFractions = new List<DasEnglishFraction>();

        foreach (var empRef in empRefs)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@empRef", empRef, DbType.String);

            var currentFraction = await _db.Value.Database.GetDbConnection().QueryAsync<DasEnglishFraction>(
                sql: "[employer_financial].[GetCurrentFractionForScheme]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);

            currentFractions.Add(currentFraction.FirstOrDefault());
        }

        return currentFractions;
    }

    public async Task<GetAccountsResponse> GetAccounts(int pageSize, int pageNumber)
    {
        var totalCount = await _db.Value.Accounts.CountAsync();

        var accounts = await _db.Value.Accounts
            .OrderBy(ac => ac.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new GetAccountsResponse
        {
            Accounts = accounts,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Account> GetAccountById(long accountId)
    {
        return await _db.Value.Accounts
            .SingleOrDefaultAsync(ac => ac.Id == accountId);
    }
}