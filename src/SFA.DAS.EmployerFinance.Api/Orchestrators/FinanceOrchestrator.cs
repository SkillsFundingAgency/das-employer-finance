using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountBalances;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;
using SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndPeriod;
using SFA.DAS.EmployerFinance.Queries.GetTransferAllowance;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class FinanceOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<FinanceOrchestrator> _logger;
    private readonly IMapper _mapper;
    private readonly IEncodingService _encodingService;

    public FinanceOrchestrator(
        IMediator mediator,
        ILogger<FinanceOrchestrator> logger,
        IMapper mapper,
        IEncodingService encodingService)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
        _encodingService = encodingService;
    }

    public async Task<List<LevyDeclaration>> GetLevy(string hashedAccountId)
    {
        _logger.LogInformation("Requesting levy declaration for account {HashedAccountId}", hashedAccountId);

        var response = await _mediator.Send(new GetLevyDeclarationRequest { HashedAccountId = hashedAccountId });
        if (response.Declarations == null)
        {
            return null;
        }

        var levyDeclarations = response.Declarations.Select(x => _mapper.Map<LevyDeclaration>(x)).ToList();
        levyDeclarations.ForEach(x => x.HashedAccountId = hashedAccountId);
        _logger.LogInformation("Received response for levy declaration for account {HashedAccountId}", hashedAccountId);

        return levyDeclarations;
    }

    public async Task<List<LevyDeclaration>> GetLevy(string hashedAccountId, string payrollYear, short payrollMonth)
    {
        _logger.LogInformation("Requesting levy declaration for account {HashedAccountId}, year {PayrollYear} and month {PayrollMonth}", hashedAccountId, payrollYear, payrollMonth);

        var response = await _mediator.Send(new GetLevyDeclarationsByAccountAndPeriodRequest { HashedAccountId = hashedAccountId, PayrollYear = payrollYear, PayrollMonth = payrollMonth });
        if (response.Declarations == null)
        {
            return null;
        }

        var levyDeclarations = response.Declarations.Select(x => _mapper.Map<LevyDeclaration>(x)).ToList();
        levyDeclarations.ForEach(x => x.HashedAccountId = hashedAccountId);
        _logger.LogInformation("Received response for levy declaration for account  {HashedAccountId}, year {PayrollYear} and month {PayrollMonth}", hashedAccountId, payrollYear, payrollMonth);
        return levyDeclarations;
    }

    public async Task<List<DasEnglishFraction>> GetEnglishFractionHistory(string hashedAccountId, string empRef)
    {
        _logger.LogInformation("Requesting english fraction history for account {HashedAccountId}, empRef {EmpRef}", hashedAccountId, empRef);

        var response = await _mediator.Send(new GetEnglishFractionHistoryQuery { HashedAccountId = hashedAccountId, EmpRef = empRef });
        if (response.FractionDetail == null)
        {
            return null;
        }

        var dasEnglishFractions = response.FractionDetail.Select(x => _mapper.Map<DasEnglishFraction>(x)).ToList();
        _logger.LogInformation("Received response for english fraction history for account {HashedAccountId}, empRef {EmpRef}", hashedAccountId, empRef);
        return dasEnglishFractions;
    }

    public async Task<List<DasEnglishFraction>> GetEnglishFractionCurrent(string hashedAccountId, string[] empRefs)
    {
        _logger.LogInformation("Requesting current english fractions for account {HashedAccountId}, empRefs {EmpRefs}", hashedAccountId, string.Join(", ", empRefs));

        var response = await _mediator.Send(new GetEnglishFractionCurrentQuery { HashedAccountId = hashedAccountId, EmpRefs = empRefs });
        if (response.Fractions == null)
        {
            return null;
        }

        var dasEnglishFractions = response.Fractions.Select(x => _mapper.Map<DasEnglishFraction>(x)).ToList();
        _logger.LogInformation("Received response for current english fractions for account {HashedAccountId}, empRefs {EmpRefs}", hashedAccountId, string.Join(", ", empRefs));
        return dasEnglishFractions;
    }

    public async Task<List<AccountBalance>> GetAccountBalances(List<string> accountIds)
    {
        _logger.LogInformation("Requesting GetAccountBalances for the accounts");

        var decodedAccountIds = new List<long>();
        foreach (var id in accountIds)
        {
            try
            {
                decodedAccountIds.Add(_encodingService.Decode(id, EncodingType.AccountId));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Exception thrown while decode hashedAccountId: {Id}", id);
            }
        }

        var response = await _mediator.Send(new GetAccountBalancesRequest
        {
            AccountIds = decodedAccountIds
        });

        var result = response?.Accounts.Select(x => _mapper.Map<AccountBalance>(x)).ToList();

        _logger.LogInformation("Received response - GetAccountBalances for the accounts {AccountsCount}", response?.Accounts.Count);

        return result;
    }

    public async Task<TransferAllowance> GetTransferAllowance(string hashedAccountId)
    {
        _logger.LogInformation("Requesting GetTransferAllowance for the hashedAccountId {HashedAccountId}", hashedAccountId);
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);

        return await GetTransferAllowanceByAccountId(accountId);
    }

    public async Task<TransferAllowance> GetTransferAllowanceByAccountId(long accountId)
    {
        _logger.LogInformation("Requesting GetTransferAllowance for the accountId {accountId}", accountId);

        var response = await _mediator.Send(new GetTransferAllowanceQuery
        {
            AccountId = accountId
        });

        var result = _mapper.Map<TransferAllowance>(response.TransferAllowance);

        _logger.LogInformation("Received response - GetTransferAllowance for the accountId {accountId}", accountId);

        return result;
    }
}