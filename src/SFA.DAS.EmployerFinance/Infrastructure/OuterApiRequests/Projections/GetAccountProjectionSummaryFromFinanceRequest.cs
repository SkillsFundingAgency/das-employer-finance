using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Projections
{
    public class GetAccountProjectionSummaryFromFinanceRequest : IGetApiRequest
    {
        private readonly long _accountId;
        public string GetUrl => $"projections/finance/{_accountId}";

        public GetAccountProjectionSummaryFromFinanceRequest(long accountId)
        {
            _accountId = accountId;
        }
    }