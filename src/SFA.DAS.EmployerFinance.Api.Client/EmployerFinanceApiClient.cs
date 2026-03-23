using Newtonsoft.Json;
using SFA.DAS.EmployerFinance.Api.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Client;

public class EmployerFinanceApiClient : IEmployerFinanceApiClient
{
    private readonly IEmployerFinanceApiClientConfiguration _configuration;
    private readonly SecureHttpClient _httpClient;

    public EmployerFinanceApiClient(IEmployerFinanceApiClientConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = new SecureHttpClient(configuration);
    }

    public Task HealthCheck()
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/healthcheck";

        return _httpClient.GetAsync(url);
    }

    public async Task<List<LevyDeclaration>> GetLevyDeclarations(string hashedAccountId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts/{hashedAccountId}/levy";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<LevyDeclaration>>(json);
    }
       
    public async Task<List<LevyDeclaration>> GetLevyForPeriod(string hashedAccountId, string payrollYear, short payrollMonth)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts/{hashedAccountId}/levy/GetLevyForPeriod";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<LevyDeclaration>>(json);
    }

    public async Task<Transactions> GetTransactions(string accountId, int year, int month)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts/{accountId}/transactions/{year}/{month}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<Transactions>(json);
    }

    public async Task<List<TransactionSummary>> GetTransactionSummary(string accountId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts/{accountId}/transactions";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<TransactionSummary>>(json);
    }

    public async Task<TotalPaymentsModel> GetFinanceStatistics()
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/financestatistics";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<TotalPaymentsModel>(json);
    }

    public async Task<List<AccountBalance>> GetAccountBalances(List<string> accountIds)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts/balances";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<AccountBalance>>(json);
    }

    public async Task<TransferAllowance> GetTransferAllowance(string hashedAccountId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts/{hashedAccountId}/transferAllowance";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<TransferAllowance>(json);
    }

    public async Task<List<Account>> GetAllEmployerAccounts(int pageNumber = 1, int pageSize = 10000)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<Account>>(json);
    }

    public async Task<Account> GetAccount(long accountId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts{accountId}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<Account>(json);
    }
    
    public async Task<List<PeriodEnd>> GetAllPeriodEnds()
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/period-ends";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<PeriodEnd>>(json);
    }

    public async Task<string> CreatePeriodEnd(PeriodEnd periodEnd)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/period-ends";
        var json = await _httpClient.PostAsync(url, JsonConvert.SerializeObject(periodEnd));

        return JsonConvert.DeserializeObject<string>(json);
    }

    public async Task<PeriodEnd> GetPeriodEndByPeriodEndId(string periodEndId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/period-ends/{periodEndId}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<PeriodEnd>(json);
    }

    public async Task<List<Guid>> GetAccountPaymentIds(long accountId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/api/accounts{accountId}/payments/ids";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<Guid>>(json);
    }

    public async Task<CreateResourceResponse> CreateAuditJob(CreateAuditJobRequest request)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/jobs";
        var json = await _httpClient.PostAsync(url, JsonConvert.SerializeObject(request));

        return JsonConvert.DeserializeObject<CreateResourceResponse>(json);
    }

    public async Task<CreateResourceResponse> CreateWorkflowLog(string jobId, string workflowId, CreateWorkflowLogRequest request)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/jobs/{jobId}/workflows/{workflowId}/logs";
        var json = await _httpClient.PostAsync(url, JsonConvert.SerializeObject(request));

        return JsonConvert.DeserializeObject<CreateResourceResponse>(json);
    }

    public async Task<List<AuditJobSummary>> GetAuditJobs(int page = 1, int pageSize = 20)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/jobs?page={page}&pagesize={pageSize}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<AuditJobSummary>>(json);
    }

    public async Task<AuditJobSummary> GetAuditJob(string jobId)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/jobs/{jobId}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<AuditJobSummary>(json);
    }

    public async Task<List<WorkflowLogEntry>> GetAuditJobLogs(string jobId, int page = 1, int pageSize = 20)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/jobs/{jobId}/logs?page={page}&pagesize={pageSize}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<WorkflowLogEntry>>(json);
    }

    public async Task<List<WorkflowLogEntry>> GetAuditWorkflowLogs(string jobId, string workflowId, int page = 1, int pageSize = 20)
    {
        var baseUrl = GetBaseUrl();
        var url = $"{baseUrl}/jobs/{jobId}/workflows/{workflowId}/logs?page={page}&pagesize={pageSize}";
        var json = await _httpClient.GetAsync(url);

        return JsonConvert.DeserializeObject<List<WorkflowLogEntry>>(json);
    }

    private string GetBaseUrl()
    {
        return _configuration.ApiBaseUrl.Trim('/');
    }   
}
