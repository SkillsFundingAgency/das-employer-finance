using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccount;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetAccountPaymentIds;
using SFA.DAS.EmployerFinance.Queries.GetAccounts;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;
using SFA.DAS.EmployerFinance.Queries.GetLastLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;
using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemesByEmployerId;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.Encoding;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class FinanceOrchestrator(
    IMediator mediator,
    ILogger<FinanceOrchestrator> logger,
    IMapper mapper,
    IEncodingService encodingService)
{
    public async Task<List<LevyDeclaration>> GetLevy(string hashedAccountId)
    {
        logger.LogInformation("Requesting levy declaration for account {HashedAccountId}", hashedAccountId);

        var response = await mediator.Send(new GetLevyDeclarationRequest { HashedAccountId = hashedAccountId });
        if (response.Declarations == null)
        {
            return null;
        }

        var levyDeclarations = response.Declarations.Select(x => mapper.Map<LevyDeclaration>(x)).ToList();
        levyDeclarations.ForEach(x => x.HashedAccountId = hashedAccountId);
        logger.LogInformation("Received response for levy declaration for account {HashedAccountId}", hashedAccountId);

        return levyDeclarations;
    }

    public async Task<List<LevyDeclaration>> GetLevy(string hashedAccountId, string payrollYear, short payrollMonth)
    {
        logger.LogInformation(
            "Requesting levy declaration for account {HashedAccountId}, year {PayrollYear} and month {PayrollMonth}",
            hashedAccountId, payrollYear, payrollMonth);

        var response = await mediator.Send(new GetLevyDeclarationsByAccountAndPeriodRequest
        {
            HashedAccountId = hashedAccountId,
            PayrollYear = payrollYear,
            PayrollMonth = payrollMonth
        });

        if (response.Declarations == null)
        {
            return null;
        }

        var levyDeclarations = response.Declarations.Select(x => mapper.Map<LevyDeclaration>(x)).ToList();
        levyDeclarations.ForEach(x => x.HashedAccountId = hashedAccountId);

        logger.LogInformation(
            "Received response for levy declaration for account {HashedAccountId}, year {PayrollYear} and month {PayrollMonth}",
            hashedAccountId, payrollYear, payrollMonth);

        return levyDeclarations;
    }

    public async Task<List<DasEnglishFraction>> GetEnglishFractionHistory(string hashedAccountId, string empRef)
    {
        logger.LogInformation("Requesting english fraction history for account {HashedAccountId}", hashedAccountId);

        var response = await mediator.Send(new GetEnglishFractionHistoryQuery
        {
            HashedAccountId = hashedAccountId,
            EmpRef = empRef
        });

        if (response.FractionDetail == null)
        {
            return null;
        }

        var dasEnglishFractions = response.FractionDetail
            .Select(x => mapper.Map<DasEnglishFraction>(x))
            .ToList();

        logger.LogInformation("Received response for english fraction history for account {HashedAccountId}", hashedAccountId);

        return dasEnglishFractions;
    }

    public async Task<List<DasEnglishFraction>> GetEnglishFractionCurrent(string hashedAccountId, string[] empRefs)
    {
        logger.LogInformation("Requesting current english fractions for account {HashedAccountId}", hashedAccountId);

        var response = await mediator.Send(new GetEnglishFractionCurrentQuery
        {
            HashedAccountId = hashedAccountId,
            EmpRefs = empRefs
        });

        if (response.Fractions == null)
        {
            return null;
        }

        var dasEnglishFractions = response.Fractions
            .Select(x => mapper.Map<DasEnglishFraction>(x))
            .ToList();

        logger.LogInformation("Received response for current english fractions for account {HashedAccountId}", hashedAccountId);

        return dasEnglishFractions;
    }

    public async Task<List<AccountBalance>> GetAccountBalances(List<string> accountIds)
    {
        logger.LogInformation("Requesting GetAccountBalances for the accounts");

        var decodedAccountIds = new List<long>();
        foreach (var id in accountIds)
        {
            try
            {
                decodedAccountIds.Add(encodingService.Decode(id, EncodingType.AccountId));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception thrown while decode hashedAccountId: {Id}", id);
            }
        }

        var response = await mediator.Send(new GetAccountBalancesRequest
        {
            AccountIds = decodedAccountIds
        });

        var result = response?.Accounts.Select(x => mapper.Map<AccountBalance>(x)).ToList();

        logger.LogInformation("Received response - GetAccountBalances for the accounts {AccountsCount}", response?.Accounts.Count);

        return result;
    }

    public async Task<TransferAllowance> GetTransferAllowance(string hashedAccountId)
    {
        logger.LogInformation("Requesting GetTransferAllowance for the hashedAccountId {HashedAccountId}", hashedAccountId);

        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);

        return await GetTransferAllowanceByAccountId(accountId);
    }

    public async Task<LevySummary> GetLevySummaryByHashedAccountId(string hashedAccountId)
    {
        logger.LogInformation("Requesting GetLevySummaryByHashedAccountId for the hashedAccountId {AccountId}", hashedAccountId);

        var response = await mediator.Send(new GetLevySummaryQuery(hashedAccountId));

        return response.Summary;
    }

    public async Task<TransferAllowance> GetTransferAllowanceByAccountId(long accountId)
    {
        logger.LogInformation("Requesting GetTransferAllowance for the accountId {AccountId}", accountId);

        var response = await mediator.Send(new GetTransferAllowanceQuery { AccountId = accountId });

        var result = mapper.Map<TransferAllowance>(response.TransferAllowance);

        logger.LogInformation("Received response - GetTransferAllowance for the accountId {AccountId}", accountId);

        return result;
    }

    public async Task<Account> GetAccountById(long accountId)
    {
        logger.LogInformation("Requesting Get Accounts for the accountId {AccountId}", accountId);

        var response = await mediator.Send(new GetAccountByIdRequest { AccountId = accountId });

        if (response?.Account == null)
        {
            return null;
        }

        var result = mapper.Map<Account>(response.Account);

        logger.LogInformation("Received response - Get Account for the accountId {AccountId}", accountId);

        return result;
    }

    public async Task<GetAccountsResponse> GetAccounts(int pageNumber, int pageSize)
    {
        var response = await mediator.Send(new GetAccountsRequest
        {
            PageSize = pageSize,
            PageNumber = pageNumber
        });

        if (response?.Accounts == null)
        {
            return null;
        }

        return response;
    }

    public async Task<GetAccountPaymentIdsResponse> GetAccountPaymentIds(long accountId, int pageNumber = 1, int pageSize = 10000)
    {
        var response = await mediator.Send(new GetAccountPaymentIdsRequest
        {
            AccountId = accountId,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        if (response?.PaymentIds == null)
        {
            return null;
        }

        return response;
    }

    public async Task<List<PayeScheme>> GetPayeSchemesByEmployerId(long accountId, string? source)
    {
        logger.LogInformation("Requesting PAYE schemes for accountId {AccountId} and source {Source}", accountId, source);

        var response = await mediator.Send(new GetPayeSchemesByEmployerIdQuery
        {
            AccountId = accountId,
            Source = source
        });

        if (response?.Schemes == null)
        {
            return null;
        }

        var result = response.Schemes.Select(x => mapper.Map<PayeScheme>(x)).ToList();

        logger.LogInformation("Received response - PAYE schemes for accountId {AccountId}: {Count}", accountId, result.Count);

        return result;
    }

    public async Task<PayeSchemeLastSubmissionDate> GetLastSubmissionDateForPayeScheme(string empRef)
    {
        logger.LogInformation("Requesting last levy submission date for empRef {EmpRef}", empRef);

        var response = await mediator.Send(new GetLastLevyDeclarationQuery { EmpRef = empRef });

        DateTime? submissionDate = response?.Transaction?.SubmissionDate;
        if (submissionDate == DateTime.MinValue)
        {
            submissionDate = null;
        }

        logger.LogInformation("Received last levy submission date for empRef {EmpRef}", empRef);

        return new PayeSchemeLastSubmissionDate
        {
            EmpRef = empRef,
            LastSubmissionDate = submissionDate
        };
    }
}